using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MovieSearch.Models;

namespace MovieSearch.ViewModels;

public class MovieSearchViewModel
{
    // User-entered title used to query OMDb.
    [Display(Name = "Movie title")]
    public string SearchTerm { get; set; } = string.Empty;

    // Holds the latest movie returned from the external API.
    public Movie? MovieResult { get; set; }

    // Collection displayed in the favorites list.
    public List<Movie> Favorites { get; set; } = new();

    // Friendly error text for the view.
    public string ErrorMessage { get; set; } = string.Empty;
}
