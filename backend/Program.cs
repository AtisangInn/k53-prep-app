using Microsoft.EntityFrameworkCore;
using K53PrepApp.Data;
using K53PrepApp.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// SQLite database
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=k53prep.db"));

// Allow the frontend (any origin in dev) to call the API
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevPolicy", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// --- Middleware ---
app.UseCors("DevPolicy");
app.UseStaticFiles();   // Serve files from wwwroot (optional)
app.MapControllers();

// --- Auto-migrate and seed on startup ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedData.Seed(db);
}

app.Run();
