using Microsoft.EntityFrameworkCore;
using K53PrepApp.Data;
using K53PrepApp.Models;

try 
{
    var builder = WebApplication.CreateBuilder(args);

    // --- Services ---
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // --- Database: PostgreSQL in production, SQLite locally ---
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    string connectionString;

    if (!string.IsNullOrEmpty(databaseUrl))
    {
        Console.WriteLine("Production: Using PostgreSQL database context...");
        try 
        {
            var uri = new Uri(databaseUrl);
            string user, password;
            var userInfo = uri.UserInfo;
            var firstColon = userInfo.IndexOf(':');
            if (firstColon >= 0)
            {
                user = userInfo.Substring(0, firstColon);
                password = userInfo.Substring(firstColon + 1);
            }
            else
            {
                user = userInfo;
                password = "";
            }
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');
            
            connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing DATABASE_URL URI, using raw string: {ex.Message}");
            connectionString = databaseUrl;
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
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine("CRITICAL ERROR DURING STARTUP:");
    Console.WriteLine(ex.ToString());
    throw; // Still rethrow to let Railway know it crashed
}
