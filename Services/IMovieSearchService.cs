using MovieSearch.Models;

namespace MovieSearch.Services;

public interface IMovieSearchService
{
    Task<Movie?> SearchMovieAsync(string title, CancellationToken cancellationToken = default);
}
