## Reflection

### What changed
- Rebuilt the console prototype into an ASP.NET Core Razor MVC app with controllers, strongly typed view models, and Bootstrap-backed views.
- Added Entity Framework Core with SQLite persistence (replacing the in-memory provider) so data survives restarts, still encapsulated through the repository layer.
- Wrapped OMDb calls in a dedicated strategy plus a caching decorator so external requests remain fast and isolated.
- Documented the solution (README) and codified configuration through strongly typed options.

### Design patterns & SRP
1. **Strategy** (`IMovieSearchStrategy` / `OmdbMovieSearchStrategy`): isolates the external API integration so the rest of the system only cares about the abstract ability to search for movies.
2. **Repository** (`IMovieRepository` / `MovieRepository`): hides EF Core specifics, keeping controllers and services focused on behavior instead of data-access plumbing.
3. **Decorator** (`CachedMovieSearchService`): provides IMemoryCache support without modifying the core `MovieSearchService`, clearly separating responsibilities.

Each component sticks to SRP—for example, the controller simply orchestrates HTTP requests, the search service coordinates the strategy + persistence, and the decorator only handles caching.

### Trade-offs
- SQLite keeps the sample self-contained; swapping to SQL Server/PostgreSQL would require new connection strings + packages but the repository/service layer remains unchanged.
- Chose IMemoryCache for the caching layer since distributed caching would complicate the sample; the decorator makes it easy to introduce Redis later.
- OMDb API key handling uses env vars or config files. For production, Azure Key Vault or user secrets would be safer, but that would overcomplicate this exercise.
- The MVC UI currently wraps the API functionality; the previous raw JSON endpoints were removed to keep the focus on Razor, but they could be reintroduced via a separate controller if needed.

### Future work
- Add background jobs to refresh cached movies proactively.
- Support search-by-ID and multi-result queries (current API endpoint only fetches the best title match).
- Introduce pagination + filtering for the favorites store once it grows large or moves to a persistent DB.
