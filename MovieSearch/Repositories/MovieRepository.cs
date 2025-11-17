using Microsoft.EntityFrameworkCore;
using MovieSearch.Data;
using MovieSearch.Models;

namespace MovieSearch.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly MovieDbContext _dbContext;

    public MovieRepository(MovieDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Movie?> GetByImdbIdAsync(string imdbId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Movies
            .AsNoTracking()
            .SingleOrDefaultAsync(m => m.ImdbID == imdbId, cancellationToken);
    }

    public async Task<IReadOnlyList<Movie>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Movies
            .AsNoTracking()
            .OrderByDescending(m => m.RetrievedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddOrUpdateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Movies
            .SingleOrDefaultAsync(m => m.ImdbID == movie.ImdbID, cancellationToken);

        if (existing is null)
        {
            await _dbContext.Movies.AddAsync(movie, cancellationToken); // Step 1a: Insert new movie.
        }
        else
        {
            _dbContext.Entry(existing).CurrentValues.SetValues(movie); // Step 1b: Update tracked entity.
        }

        await _dbContext.SaveChangesAsync(cancellationToken); // Step 2: Persist the change.
    }

    public async Task DeleteAsync(string imdbId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Movies.SingleOrDefaultAsync(m => m.ImdbID == imdbId, cancellationToken);
        if (entity is null)
        {
            return; // Nothing to delete.
        }

        _dbContext.Movies.Remove(entity); // Remove entity from context.
        await _dbContext.SaveChangesAsync(cancellationToken); // Flush deletion.
    }
}
