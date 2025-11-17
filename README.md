
## MovieSearch (ASP.NET Core Razor MVC)

An ASP.NET Core Razor MVC app that lets you search movies through the OMDb API, caches recent lookups, and stores curated favorites via Entity Framework Core. The UI is built with Razor views (Bootstrap) while the back-end keeps the strategy, repository, and decorator patterns that power the search pipeline.

### Requirements

- .NET 9 SDK
- OMDb API key (free at [omdbapi.com/apikey.aspx](https://www.omdbapi.com/apikey.aspx))

### Getting Started

1. Clone the repo and move into `MovieSearch`.
2. Restore and build:
   ```bash
   dotnet restore
   dotnet build
   ```
3. Provide the OMDb API key either through `appsettings.json` (`Omdb:ApiKey`) or as an environment variable (preferred on shared machines):
   ```bash
   export OMDB_API_KEY=YOUR_KEY_HERE
   ```
4. Run the MVC app:
   ```bash
   dotnet run
   ```
   Then browse to `https://localhost:7243` (or the HTTP URL shown in the console). The first HTTPS visit may show a self-signed certificate warning; trust the ASP.NET dev certificate or switch to the HTTP port for local testing (`http://localhost:5145` by default).

### What you get

- Razor UI (`MoviesController` + `Views/Movies/Index`) with inline comments that walk through each step of the model-view-controller flow.
- **Entity Framework Core (InMemory provider)** stores favorite movies and recent search results in `MovieDbContext`.
- **OMDb integration** lives in `OmdbMovieSearchStrategy`, keeping the external HTTP concerns isolated.
- **Caching layer** (`CachedMovieSearchService`) wraps the core search service, providing decorator-style IMemoryCache support.
- **Design patterns** (documented inline):
  1. **Strategy** (`IMovieSearchStrategy` / `OmdbMovieSearchStrategy`) encapsulates the API integration so alternate providers can be swapped in without touching controllers.
  2. **Repository** (`IMovieRepository`, `MovieRepository`) isolates EF Core from the rest of the application.
  3. **Decorator** (`CachedMovieSearchService`) adds caching behavior to the search service without modifying its core logic, honoring SRP.

### Configuration

`appsettings.json` stores the default OMDb base address and logging settings. The API key can live there for local experiments, but production runs should rely on the `OMDB_API_KEY` environment variable so secrets never hit source control.

### Database

- The app now persists data in SQLite (`MovieSearch.db` in the project root). The file is created (and migrations are applied) automatically on startup.
- To reset the database, stop the app and delete `MovieSearch.db`; the schema will be recreated on the next run.
- If you want to manage migrations manually, install the EF Core CLI (`dotnet tool install --global dotnet-ef`) and run `dotnet ef database update`.

### Development Tips

- Swap `UseInMemoryDatabase` for a persistent provider (e.g., SQLite or SQL Server) in `Program.cs` when you need durable storage.
- Because the UI uses standard MVC patterns, adding new pages or forms is as easy as adding additional controller actions + Razor views.
- The DI-first architecture makes it easy to inject additional behavior (retry strategies, alternative caching, etc.).
>>>>>>> 4739d55 (initial commit for MovieSearch Project)
