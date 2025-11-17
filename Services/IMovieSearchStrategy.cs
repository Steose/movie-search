using MovieSearch.Models;

namespace MovieSearch.Services;

public interface IMovieSearchStrategy
{
    Task<Movie?> SearchAsync(string title, CancellationToken cancellationToken = default);
}
