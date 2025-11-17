using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MovieSearch.Models;

namespace MovieSearch.Services;

public class CachedMovieSearchService : IMovieSearchService
{
    private readonly IMovieSearchService _inner;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedMovieSearchService> _logger;
    private readonly MemoryCacheEntryOptions _cacheEntryOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    public CachedMovieSearchService(
        IMovieSearchService inner,
        IMemoryCache cache,
        ILogger<CachedMovieSearchService> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Movie?> SearchMovieAsync(string title, CancellationToken cancellationToken = default)
    {
        var normalizedTitle = title?.Trim(); // Step 0: Normalize key input for consistent caching.
        if (string.IsNullOrWhiteSpace(normalizedTitle))
        {
            return await _inner.SearchMovieAsync(title ?? string.Empty, cancellationToken); // No caching when key is empty.
        }

        var cacheKey = $"movie-search::{normalizedTitle!.ToLowerInvariant()}"; // Step 1: Build a deterministic cache key.
        if (_cache.TryGetValue(cacheKey, out Movie? cached) && cached is not null)
        {
            _logger.LogDebug("Cache hit for {Title}", title); // Step 2a: Cache hit -> log for observability.
            return cached; // Step 2b: Return cached copy immediately.
        }

        // Step 3: Cache miss -> delegate to the inner service.
        var result = await _inner.SearchMovieAsync(title ?? string.Empty, cancellationToken);
        if (result is not null)
        {
            _cache.Set(cacheKey, result, _cacheEntryOptions); // Step 4: Store result for future lookups.
        }

        return result; // Step 5: Bubble result back up the call stack.
    }
}
