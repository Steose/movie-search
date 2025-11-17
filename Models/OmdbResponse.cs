
using System.Text.Json.Serialization;

namespace MovieSearch.Models;

public class OmdbResponse
{
    public string Title { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Director { get; set; } = string.Empty;
    public string Plot { get; set; } = string.Empty;
    public string Poster { get; set; } = string.Empty;
    [JsonPropertyName("imdbRating")]
    public string ImdbRating { get; set; } = string.Empty;
    [JsonPropertyName("imdbID")]
    public string ImdbID { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty; // "True" eller "False"
    public string Error { get; set; } = string.Empty;
}
