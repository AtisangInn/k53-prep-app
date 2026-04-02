using Microsoft.EntityFrameworkCore;
using K53PrepApp.Data;
using K53PrepApp.Models;

try 
{
    // Enable legacy timestamp behavior for PostgreSQL compatibility (cross-platform with SQLite)
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

    var builder = WebApplication.CreateBuilder(args);

    // --- Services ---
    builder.Services.AddControllers()
        .AddJsonOptions(options => {
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        });
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
            
            connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password};SslMode=Require;TrustServerCertificate=true;";
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
    
    // Enable serving index.html as a default file
    app.UseDefaultFiles();
    app.UseStaticFiles();
    
    app.MapControllers();


    // --- Auto-migrate and seed on startup ---
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            // --- Emergency Reset Logic ---
            if (Environment.GetEnvironmentVariable("RESET_DB") == "true")
            {
                Console.WriteLine("!!! NUCLEAR RESET: Wiping schema for PostgreSQL stability...");
                try 
                {
                    // This is the definitive way to wipe a Postgres DB when tables are stuck
                    await db.Database.ExecuteSqlRawAsync("DROP SCHEMA public CASCADE; CREATE SCHEMA public;");
                    Console.WriteLine("!!! DB PURGED SUCCESSFULLY.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"!!! PURGE WARNING: {ex.Message}");
                    // Fallback to polite delete if raw SQL fails
                    db.Database.EnsureDeleted();
                }
            }

            Console.WriteLine("!!! VERSION 5.0 - NUCLEAR POSTGRES STABILIZATION ACTIVE !!!");
            Console.WriteLine("--- Database Sync Started ---");
            var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();
            Console.WriteLine($"Pending migrations: {pending.Count} ({string.Join(", ", pending)})");

            if (pending.Any())
            {
                Console.WriteLine("Applying migrations...");
                await db.Database.MigrateAsync();
                Console.WriteLine("Migrations applied successfully.");
            }
            else
            {
                Console.WriteLine("No pending migrations found. Verifying tables exist...");
                // Fallback for missing tables despite no pending migrations
                db.Database.EnsureCreated();
            }

            SeedData.Seed(db);
            Console.WriteLine("--- Database Sync Completed Successfully ---");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"!!! DATABASE ERROR during startup: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"!!! INNER ERROR: {ex.InnerException.Message}");
            }
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
