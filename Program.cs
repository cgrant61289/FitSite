using FitSite.Components;
using FitSite.Data;
using FitSite.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

static bool IsPostgresConnectionString(string cs)
{
    if (string.IsNullOrWhiteSpace(cs)) return false;
    var trimmed = cs.Trim();
    return trimmed.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
        || trimmed.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
        || trimmed.Contains("Host=", StringComparison.OrdinalIgnoreCase);
}

static string ToNpgsqlConnectionString(string cs)
{
    var trimmed = cs.Trim();
    if (!trimmed.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
        && !trimmed.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        return cs; // already key=value format

    var uri = new Uri(trimmed);
    var userInfo = uri.UserInfo.Split(':', 2);
    var user = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
    var host = uri.Host;
    var port = uri.Port > 0 ? uri.Port : 5432;
    var database = uri.AbsolutePath.TrimStart('/');

    return $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}

// Add Entity Framework and SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing connection string: DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (IsPostgresConnectionString(connectionString))
        options.UseNpgsql(ToNpgsqlConnectionString(connectionString));
    else
        options.UseSqlite(connectionString);
});

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

// Map API endpoints for dashboard
app.MapGet("/api/dashboard/stats/{userId}", async (string userId, AppDbContext db) =>
{
    var lastSession = await db.WorkoutSessions
        .Where(w => w.UserId == userId)
        .OrderByDescending(w => w.CompletedUtc)
        .FirstOrDefaultAsync();

    var thisMonth = DateTime.UtcNow.AddMonths(-1);
    var workoutsThisMonth = await db.WorkoutSessions
        .Where(w => w.UserId == userId && w.StartedUtc >= thisMonth)
        .CountAsync();

    var totalCaloriesThisMonth = await db.WorkoutSessions
        .Where(w => w.UserId == userId && w.StartedUtc >= thisMonth)
        .SumAsync(w => (long)w.CaloriesBurned);

    var weightData = await db.ProgressEntries
        .Where(p => p.UserId == userId)
        .OrderByDescending(p => p.RecordedUtc)
        .Take(12)
        .ToListAsync();

    return Results.Ok(new
    {
        lastWorkout = lastSession?.CompletedUtc,
        workoutsThisMonth,
        totalCaloriesThisMonth,
        currentWeight = weightData.FirstOrDefault()?.WeightLbs,
        weightHistory = weightData.Select(w => new { date = w.RecordedUtc, weight = w.WeightLbs }).ToList()
    });
});

app.MapGet("/api/dashboard/recent-workouts/{userId}", async (string userId, AppDbContext db) =>
{
    var workouts = await db.WorkoutSessions
        .Where(w => w.UserId == userId && w.CompletedUtc.HasValue)
        .OrderByDescending(w => w.CompletedUtc)
        .Take(5)
        .Select(w => new
        {
            w.Id,
            w.Name,
            w.DurationMinutes,
            w.CaloriesBurned,
            completedDate = w.CompletedUtc
        })
        .ToListAsync();

    return Results.Ok(workouts);
});

app.MapGet("/api/dashboard/personal-records/{userId}", async (string userId, AppDbContext db) =>
{
    var records = await db.ExerciseLogs
        .Where(e => e.UserId == userId)
        .ToListAsync();

    var result = records
        .GroupBy(e => e.ExerciseName)
        .Select(g => new
        {
            exercise = g.Key,
            maxWeight = g.Max(e => e.WeightLbs),
            maxReps = g.Max(e => e.Reps)
        })
        .ToList();

    return Results.Ok(result);
});

app.MapGet("/api/dashboard/exercise-progression/{userId}", async (string userId, AppDbContext db) =>
{
    var points = await db.ExerciseLogs
        .Where(e => e.UserId == userId)
        .OrderBy(e => e.LoggedUtc)
        .Select(e => new
        {
            e.ExerciseName,
            e.LoggedUtc,
            e.WeightLbs
        })
        .ToListAsync();

    var grouped = points
        .GroupBy(p => p.ExerciseName)
        .Select(g => new
        {
            exercise = g.Key,
            points = g
                .OrderBy(x => x.LoggedUtc)
                .Select(x => new
                {
                    loggedUtc = x.LoggedUtc,
                    weightLbs = x.WeightLbs
                })
                .ToList()
        })
        .OrderBy(x => x.exercise)
        .ToList();

    return Results.Ok(grouped);
});

app.MapGet("/logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.LocalRedirect("/");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .DisableAntiforgery();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var cs = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";

    if (IsPostgresConnectionString(cs))
    db.Database.Migrate();
else
    db.Database.EnsureCreated();
}

app.Run();