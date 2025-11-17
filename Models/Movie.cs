using System.ComponentModel.DataAnnotations;

namespace MovieSearch.Models;

public class Movie
{
    [Key]
    public string ImdbID { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Director { get; set; } = string.Empty;
    public string Plot { get; set; } = string.Empty;
    public string Poster { get; set; } = string.Empty;
    public string ImdbRating { get; set; } = string.Empty;
    public DateTime RetrievedAtUtc { get; set; } = DateTime.UtcNow;
}
