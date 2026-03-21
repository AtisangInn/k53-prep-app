using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using K53PrepApp.Data;
using K53PrepApp.Models;

namespace K53PrepApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    // Read admin code from environment variable, fall back to appsettings.json for local dev
    private string AdminCode => Environment.GetEnvironmentVariable("ADMIN_CODE")
                                ?? _config["AdminCode"]
                                ?? "admin1234";

    public QuestionsController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    // GET /api/questions - all active questions (optionally filtered by category)
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category)
    {
        var query = _db.Questions.Where(q => q.IsActive);
        if (!string.IsNullOrEmpty(category))
            query = query.Where(q => q.Category == category);

        var questions = await query
            .OrderBy(q => q.Category)
            .ThenBy(q => q.SubCategory)
            .ThenBy(q => q.Id)
            .Select(q => new {
                q.Id, q.Category, q.SubCategory, q.Text, q.ImageUrl,
                q.OptionA, q.OptionB, q.OptionC, q.OptionD, q.Explanation,
                q.CorrectOption
            })
            .ToListAsync();

        return Ok(questions);
    }

    // GET /api/questions/test - returns a shuffled test set (64 questions: 28 Rules, 28 Signs, 8 Controls)
    [HttpGet("test")]
    public async Task<IActionResult> GetTestQuestions()
    {
        var rules    = await GetRandomQuestions("Rules", 28);
        var signs    = await GetRandomQuestions("Signs", 28);
        var controls = await GetRandomQuestions("Controls", 8);

        var allQuestions = rules.Concat(signs).Concat(controls)
            .OrderBy(_ => Guid.NewGuid()) // shuffle
            .Select(q => new {
                q.Id, q.Category, q.SubCategory, q.Text, q.ImageUrl,
                q.OptionA, q.OptionB, q.OptionC, q.OptionD
                // CorrectOption NOT sent to client during test
            });

        return Ok(allQuestions);
    }

    // GET /api/questions/{id}/answer - get the correct answer (for study mode review)
    [HttpGet("{id}/answer")]
    public async Task<IActionResult> GetAnswer(int id)
    {
        var q = await _db.Questions.FindAsync(id);
        if (q == null) return NotFound();
        return Ok(new { q.Id, q.CorrectOption, q.Explanation });
    }

    // POST /api/questions - admin: create question
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] QuestionDto dto, [FromHeader(Name="X-Admin-Code")] string? adminCode)
    {
        if (adminCode != AdminCode) return Unauthorized();

        var q = new Question {
            Category = dto.Category, SubCategory = dto.SubCategory, Text = dto.Text,
            ImageUrl = dto.ImageUrl, OptionA = dto.OptionA, OptionB = dto.OptionB,
            OptionC = dto.OptionC, OptionD = dto.OptionD, CorrectOption = dto.CorrectOption,
            Explanation = dto.Explanation
        };
        _db.Questions.Add(q);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = q.Id }, q);
    }

    // PUT /api/questions/{id} - admin: update question
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] QuestionDto dto, [FromHeader(Name="X-Admin-Code")] string? adminCode)
    {
        if (adminCode != AdminCode) return Unauthorized();

        var q = await _db.Questions.FindAsync(id);
        if (q == null) return NotFound();

        q.Category = dto.Category; q.SubCategory = dto.SubCategory; q.Text = dto.Text;
        q.ImageUrl = dto.ImageUrl; q.OptionA = dto.OptionA; q.OptionB = dto.OptionB;
        q.OptionC = dto.OptionC; q.OptionD = dto.OptionD; q.CorrectOption = dto.CorrectOption;
        q.Explanation = dto.Explanation;
        await _db.SaveChangesAsync();
        return Ok(q);
    }

    // DELETE /api/questions/{id} - admin: soft-delete question
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, [FromHeader(Name="X-Admin-Code")] string? adminCode)
    {
        if (adminCode != AdminCode) return Unauthorized();

        var q = await _db.Questions.FindAsync(id);
        if (q == null) return NotFound();
        q.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<BulkImportResponse>> BulkImport([FromBody] List<QuestionDto> questions)
    {
        var adminCode = Request.Headers["X-Admin-Code"].ToString();
        var expectedCode = _config["AdminCode"] ?? "admin1234";

        if (adminCode != expectedCode)
            return Unauthorized("Invalid admin code");

        // Soft-delete all existing active questions
        var existing = await _db.Questions.Where(q => q.IsActive).ToListAsync();
        foreach (var q in existing)
        {
            q.IsActive = false;
        }

        // Add new questions
        var newQuestions = questions.Select(dto => new Question
        {
            Category = dto.Category,
            SubCategory = dto.SubCategory,
            Text = dto.Text,
            OptionA = dto.OptionA,
            OptionB = dto.OptionB,
            OptionC = dto.OptionC,
            OptionD = dto.OptionD,
            CorrectOption = dto.CorrectOption,
            Explanation = dto.Explanation,
            ImageUrl = dto.ImageUrl,
            IsActive = true
        }).ToList();

        _db.Questions.AddRange(newQuestions);
        await _db.SaveChangesAsync();

        return Ok(new BulkImportResponse
        {
            DeletedCount = existing.Count,
            ImportedCount = newQuestions.Count,
            Message = "Bulk replacement completed successfully."
        });
    }

    // ---- helpers ----
    private async Task<List<Question>> GetRandomQuestions(string category, int count)
    {
        var all = await _db.Questions
            .Where(q => q.IsActive && q.Category == category)
            .ToListAsync();

        return all.OrderBy(_ => Guid.NewGuid()).Take(count).ToList();
    }
}

public record QuestionDto(
    string Category, string SubCategory, string Text,
    string? ImageUrl, string OptionA, string OptionB,
    string OptionC, string OptionD, string CorrectOption, string Explanation);

public class BulkImportResponse
{
    public int DeletedCount { get; set; }
    public int ImportedCount { get; set; }
    public string Message { get; set; } = "";
}
