using System.Net.Http;
using Microsoft.Extensions.Logging;
using MovieSearch.Models;
using MovieSearch.Repositories;

namespace MovieSearch.Services;

public class MovieSearchService : IMovieSearchService
{
    private readonly IMovieSearchStrategy _movieSearchStrategy;
    private readonly IMovieRepository _movieRepository;
    private readonly ILogger<MovieSearchService> _logger;

    public MovieSearchService(
        IMovieSearchStrategy movieSearchStrategy,
        IMovieRepository movieRepository,
        ILogger<MovieSearchService> logger)
    {
        _movieSearchStrategy = movieSearchStrategy;
        _movieRepository = movieRepository;
        _logger = logger;
    }

    public async Task<Movie?> SearchMovieAsync(string title, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return null; // Step 0: Do nothing when there is no title.
        }

        try
        {
            // Step 1: Ask the search strategy (OMDb) for a movie.
            var movie = await _movieSearchStrategy.SearchAsync(title.Trim(), cancellationToken);
            if (movie is null)
            {
                return null; // Step 2: Bubble up that nothing was found.
            }

            // Step 3: Persist the successful result so favorites stay current.
            await _movieRepository.AddOrUpdateAsync(movie, cancellationToken);
            return movie; // Step 4: Return the data back to the controller/view.
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while searching for {Title}", title);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Configuration error while searching for {Title}", title);
            throw;
        }
    }
}
