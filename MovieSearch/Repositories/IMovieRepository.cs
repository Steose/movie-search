using MovieSearch.Models;

namespace MovieSearch.Repositories;

public interface IMovieRepository
{
    Task<Movie?> GetByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Movie>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddOrUpdateAsync(Movie movie, CancellationToken cancellationToken = default);
    Task DeleteAsync(string imdbId, CancellationToken cancellationToken = default);
}
