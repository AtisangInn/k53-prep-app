using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using K53PrepApp.Data;
using K53PrepApp.Models;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace K53PrepApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public PaymentsController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    // ── Config helpers ──────────────────────────────────────────────────────
    private string MerchantId  => Environment.GetEnvironmentVariable("PAYFAST_MERCHANT_ID")
                                   ?? _config["PayFast:MerchantId"]
                                   ?? "10000100"; // PayFast sandbox default

    private string MerchantKey => Environment.GetEnvironmentVariable("PAYFAST_MERCHANT_KEY")
                                   ?? _config["PayFast:MerchantKey"]
                                   ?? "46f0cd694581a"; // PayFast sandbox default

    private string Passphrase  => Environment.GetEnvironmentVariable("PAYFAST_PASSPHRASE")
                                   ?? _config["PayFast:Passphrase"]
                                   ?? "";

    private bool IsSandbox     => (Environment.GetEnvironmentVariable("PAYFAST_SANDBOX") ?? _config["PayFast:Sandbox"]) == "true";

    private string PayFastUrl  => IsSandbox
                                   ? "https://sandbox.payfast.co.za/eng/process"
                                   : "https://www.payfast.co.za/eng/process";

    private string AppBase     => Environment.GetEnvironmentVariable("APP_BASE_URL")
                                   ?? _config["AppBaseUrl"]
                                   ?? "https://k53-prep-app.vercel.app";

    private string ApiBase     => Environment.GetEnvironmentVariable("API_BASE_URL")
                                   ?? _config["ApiBaseUrl"]
                                   ?? "https://k53-prep-app-production.up.railway.app";

    // ── GET /api/payments/student/{id}/status ───────────────────────────────
    [HttpGet("student/{id}/status")]
    public async Task<IActionResult> GetStatus(int id)
    {
        var student = await _db.Students.FindAsync(id);
        if (student == null) return NotFound();

        // Auto-expire premium
        // Calculate seconds until next UTC midnight reset
        var now = DateTime.UtcNow;
        var nextMidnight = now.Date.AddDays(1);
        var secondsUntilRefresh = (int)(nextMidnight - now).TotalSeconds;

        // Reset daily limits if it's a new day
        var today = now.Date;
        if (student.LastFreeFlipDate?.Date != today)
        {
            student.FreeFlipsToday = 0;
            student.FreeNextsToday = 0;
            student.FreeTestsUsed = 0;
            student.LastFreeFlipDate = today;
            await _db.SaveChangesAsync();
        }

        return Ok(new
        {
            isPremium          = student.IsPremium,
            premiumUntil       = student.PremiumUntil,
            freeTestsUsed      = student.FreeTestsUsed,
            freeTestsRemaining = Math.Max(0, 2 - student.FreeTestsUsed),
            freeFlipsToday     = student.FreeFlipsToday,
            freeNextsToday     = student.FreeNextsToday,
            freeFlipsRemaining = Math.Max(0, 10 - student.FreeFlipsToday),
            freeNextsRemaining = Math.Max(0, 30 - student.FreeNextsToday),
            secondsUntilRefresh
        });
    }

    // ── POST /api/payments/student/{id}/checkout ────────────────────────────
    // Returns the PayFast URL + form fields so the frontend can auto-submit
    [HttpPost("student/{id}/checkout")]
    public async Task<IActionResult> CreateCheckout(int id)
    {
        var student = await _db.Students.FindAsync(id);
        if (student == null) return NotFound();

        if (student.IsPremium)
            return BadRequest(new { message = "Student already has premium access." });

        // Create a pending payment record
        var mPaymentId = Guid.NewGuid().ToString("N")[..20];
        var payment = new StudentPayment
        {
            StudentId  = id,
            MPaymentId = mPaymentId,
            Amount     = 79.00m,
            Status     = "pending"
        };
        _db.StudentPayments.Add(payment);
        await _db.SaveChangesAsync();

        // Build PayFast fields (order matters for signature)
        var nameParts  = student.Name.Trim().Split(' ', 2);
        var firstName  = nameParts[0];
        var lastName   = nameParts.Length > 1 ? nameParts[1] : "-";

        var fields = new List<KeyValuePair<string, string>>
        {
            // Merchant
            new("merchant_id",   MerchantId),
            new("merchant_key",  MerchantKey),
            new("return_url",    $"{AppBase}/payment-success.html"),
            new("cancel_url",    $"{AppBase}/payment-cancel.html"),
            new("notify_url",    $"{ApiBase}/api/payments/notify"),
            // Buyer
            new("name_first",    firstName),
            new("name_last",     lastName),
            // Transaction
            new("m_payment_id",  mPaymentId),
            new("amount",        "79.00"),
            new("item_name",     "K53 Academy Premium"),
            new("item_description", "30 Days Unlimited Mock Exams and Handbook"),
            // Custom data
            new("custom_int1",   id.ToString())
        };

        var signature = GenerateSignature(fields);
        fields.Add(new("signature", signature));

        return Ok(new { 
            url = PayFastUrl, 
            fields = fields.Select(f => new { name = f.Key, value = f.Value }).ToList()
        });
    }

    // ── POST /api/payments/notify ────────────────────────────────────────────
    // PayFast ITN (Instant Transaction Notification) webhook
    // PayFast retries this if we don't return 200, so always return 200
    [HttpPost("notify")]
    public async Task<IActionResult> Notify()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // Parse URL-encoded body preserving order
            var itnParams = body.Split('&')
                .Select(p => p.Split('=', 2))
                .Where(p => p.Length == 2)
                .Select(p => new KeyValuePair<string, string>(
                    p[0], // Keep original keys URL encoded or decoded? PayFast expects Unescaped for processing, but URL encoded for signature
                    Uri.UnescapeDataString(p[1].Replace('+', ' '))
                )).ToList();

            var itnDict = itnParams.ToDictionary(k => Uri.UnescapeDataString(k.Key), k => k.Value);

            // 1. Validate signature. Build signature string in exact order received.
            var receivedSig  = itnDict.GetValueOrDefault("signature", "");
            
            var paramString = string.Join("&", itnParams
                .Where(k => Uri.UnescapeDataString(k.Key) != "signature")
                .Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));

            if (!string.IsNullOrEmpty(Passphrase))
                paramString += $"&passphrase={HttpUtility.UrlEncode(Passphrase)}";

            var hash = MD5.HashData(Encoding.UTF8.GetBytes(paramString));
            var expectedSig = Convert.ToHexString(hash).ToLowerInvariant();

            if (!string.Equals(receivedSig, expectedSig, StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"[PayFast] Signature mismatch. Expected={expectedSig} Got={receivedSig}");
                return Ok(); // Still 200 to stop retries
            }

            // 2. Validate amount
            if (!decimal.TryParse(itnDict.GetValueOrDefault("amount_gross", "0"),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var amount)
                || amount != 79.00m)
            {
                Console.WriteLine($"[PayFast] Invalid amount: {amount}");
                return Ok();
            }

            // 3. Find payment record
            var mPaymentId    = itnDict.GetValueOrDefault("m_payment_id", "");
            var pfPaymentId   = itnDict.GetValueOrDefault("pf_payment_id", "");
            var paymentStatus = itnDict.GetValueOrDefault("payment_status", "");

            var payment = await _db.StudentPayments
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.MPaymentId == mPaymentId);

            if (payment == null)
            {
                Console.WriteLine($"[PayFast] No payment record for m_payment_id={mPaymentId}");
                return Ok();
            }

            // 4. Update payment
            payment.PfPaymentId = pfPaymentId;
            payment.Status      = paymentStatus.ToLower();

            if (paymentStatus == "COMPLETE")
            {
                payment.CompletedAt      = DateTime.UtcNow;
                payment.Student.IsPremium    = true;
                payment.Student.PremiumUntil = DateTime.UtcNow.AddDays(30);
                Console.WriteLine($"[PayFast] Premium activated for student {payment.StudentId} until {payment.Student.PremiumUntil}");
            }

            await _db.SaveChangesAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PayFast] ITN error: {ex.Message}");
            return Ok(); // Always 200 to PayFast
        }
    }

    // ── Signature helper ─────────────────────────────────────────────────────
    private string GenerateSignature(IEnumerable<KeyValuePair<string, string>> fields)
    {
        // Build param string: key=value pairs URL-encoded, joined with &
        var paramString = string.Join("&", fields.Select(
            kvp => $"{HttpUtility.UrlEncode(kvp.Key)}={HttpUtility.UrlEncode(kvp.Value)}"
        ));

        // Append passphrase if configured
        if (!string.IsNullOrEmpty(Passphrase))
            paramString += $"&passphrase={HttpUtility.UrlEncode(Passphrase)}";

        var hash = MD5.HashData(Encoding.UTF8.GetBytes(paramString));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
