using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MovieSearch.Data;
using MovieSearch.Options;
using MovieSearch.Repositories;
using MovieSearch.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// Add MVC services so we can render Razor views alongside controllers.
builder.Services.AddControllersWithViews();

builder.Services.Configure<OmdbOptions>(options =>
{
    builder.Configuration.GetSection(OmdbOptions.SectionName).Bind(options);
    options.BaseAddress = string.IsNullOrWhiteSpace(options.BaseAddress)
        ? "https://www.omdbapi.com/"
        : options.BaseAddress;

    if (string.IsNullOrWhiteSpace(options.ApiKey))
    {
        options.ApiKey = builder.Configuration["OMDB_API_KEY"] ?? string.Empty;
    }
});

var rawConnectionString = builder.Configuration.GetConnectionString("Default")
                         ?? "Data Source=MovieSearch.db"; // Default to local SQLite file for portability.

var sqliteBuilder = new SqliteConnectionStringBuilder(rawConnectionString);

// Ensure the SQLite file path is absolute so the DB ends up in the project root.
if (!Path.IsPathRooted(sqliteBuilder.DataSource))
{
    sqliteBuilder.DataSource = Path.Combine(builder.Environment.ContentRootPath, sqliteBuilder.DataSource);
}

Directory.CreateDirectory(Path.GetDirectoryName(sqliteBuilder.DataSource)!); // Create folder when nested paths are used.
var databasePath = sqliteBuilder.DataSource;
var resolvedConnectionString = sqliteBuilder.ToString();

builder.Services.AddDbContext<MovieDbContext>(dbOptions =>
    dbOptions.UseSqlite(resolvedConnectionString)); // Use SQLite instead of the in-memory provider.

builder.Services.AddMemoryCache(); // Enables IMemoryCache for the caching decorator.
builder.Services.AddScoped<IMovieRepository, MovieRepository>(); // Repository keeps EF Core concerns isolated.
builder.Services.AddHttpClient<IMovieSearchStrategy, OmdbMovieSearchStrategy>((serviceProvider, client) =>
{
    // Configure the typed client every time it is resolved so BaseAddress and headers are always set.
    var options = serviceProvider.GetRequiredService<IOptionsMonitor<OmdbOptions>>().CurrentValue;
    client.BaseAddress = new Uri(options.BaseAddress);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
builder.Services.AddScoped<MovieSearchService>();
builder.Services.AddScoped<IMovieSearchService>(sp =>
{
    var inner = sp.GetRequiredService<MovieSearchService>();
    var cache = sp.GetRequiredService<IMemoryCache>();
    var logger = sp.GetRequiredService<ILogger<CachedMovieSearchService>>();
    return new CachedMovieSearchService(inner, cache, logger);
});

var app = builder.Build(); // Build the web host with all configured services.
app.Logger.LogInformation("SQLite database path: {Path}", databasePath);

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MovieDbContext>();
    db.Database.Migrate(); // Apply pending migrations so SQLite schema stays up-to-date.
}

app.UseHttpsRedirection(); // Force HTTPS so browsers hit the secure endpoint.
app.UseStaticFiles(); // Serve css/js from wwwroot for the Razor views.

app.UseRouting(); // Enable endpoint routing for MVC.

app.UseAuthorization(); // Placeholder for future auth; keeps pipeline standard.

// Map standard MVC route so / resolves to MoviesController.Index.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Movies}/{action=Index}/{id?}");

app.Run();
