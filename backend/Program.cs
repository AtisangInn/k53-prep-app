using Microsoft.EntityFrameworkCore;
using K53PrepApp.Data;
using K53PrepApp.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- Database: PostgreSQL in production, SQLite locally ---
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Production: Using PostgreSQL database...");
    if (connectionString.StartsWith("postgres://"))
    {
        try 
        {
            var uri = new Uri(connectionString);
            var userInfo = uri.UserInfo.Split(':');
            var user = userInfo[0];
            var pass = userInfo.Length > 1 ? userInfo[1] : "";
            connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={user};Password={pass};SSL Mode=Require;Trust Server Certificate=true";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing DATABASE_URL: {ex.Message}");
            // Fall back to sqlite or let it fail gracefully
        }
    }
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connectionString));
}
else
{
    Console.WriteLine("Development: Using SQLite database...");
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=k53prep.db"));
}

// --- CORS ---
var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevPolicy", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

    if (!string.IsNullOrEmpty(frontendUrl))
    {
        Console.WriteLine($"Production CORS: Allowing {frontendUrl}");
        options.AddPolicy("ProductionPolicy", policy =>
            policy.WithOrigins(frontendUrl).AllowAnyMethod().AllowAnyHeader());
    }
});

var app = builder.Build();

// --- Middleware ---
var isProd = app.Environment.IsProduction();
app.UseCors(isProd && !string.IsNullOrEmpty(frontendUrl) ? "ProductionPolicy" : "DevPolicy");
app.UseStaticFiles();
app.MapControllers();

// Healthy check
app.MapGet("/", () => "K53 Prep API is running!");

// --- Auto-migrate and seed on startup ---
using (var scope = app.Services.CreateScope())
{
    try 
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Console.WriteLine("Initializing database...");
        db.Database.EnsureCreated();
        SeedData.Seed(db);
        Console.WriteLine("Database initialized and seeded.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"FATAL: Database initialization failed: {ex.Message}");
        if (ex.InnerException != null) 
            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
    }
}

app.Run();
