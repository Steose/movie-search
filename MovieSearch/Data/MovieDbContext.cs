using Microsoft.EntityFrameworkCore;
using MovieSearch.Models;

namespace MovieSearch.Data;

public class MovieDbContext : DbContext
{
    public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options)
    {
    }

    public DbSet<Movie> Movies => Set<Movie>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movie>()
            .HasKey(m => m.ImdbID);

        base.OnModelCreating(modelBuilder);
    }
}
