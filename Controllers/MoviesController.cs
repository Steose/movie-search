using Microsoft.AspNetCore.Mvc;
using System.Linq;
using MovieSearch.Repositories;
using MovieSearch.Services;
using MovieSearch.ViewModels;

namespace MovieSearch.Controllers;

public class MoviesController : Controller
{
    private readonly IMovieSearchService _movieSearchService;
    private readonly IMovieRepository _movieRepository;

    public MoviesController(
        IMovieSearchService movieSearchService,
        IMovieRepository movieRepository)
    {
        _movieSearchService = movieSearchService;
        _movieRepository = movieRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        // Step 1: Load existing favorites so the landing page always shows persisted movies.
        var favorites = await _movieRepository.GetAllAsync(cancellationToken);

        // Step 2: Build the view model with the favorites collection.
        var viewModel = new MovieSearchViewModel
        {
            Favorites = favorites.ToList()
        };

        // Step 3: Render the Razor page with the populated model.
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Search(MovieSearchViewModel model, CancellationToken cancellationToken)
    {
        // Step 1: Guard against empty searches and immediately reload favorites for feedback.
        if (string.IsNullOrWhiteSpace(model.SearchTerm))
        {
            ModelState.AddModelError(nameof(model.SearchTerm), "Please provide a movie title.");
            model.Favorites = (await _movieRepository.GetAllAsync(cancellationToken)).ToList();
            return View("Index", model);
        }

        try
        {
            // Step 2: Delegate to the search service (with caching + repository persistence baked in).
            var movie = await _movieSearchService.SearchMovieAsync(model.SearchTerm, cancellationToken);

            if (movie is null)
            {
                // Step 3a: Communicate if OMDb did not return a result.
                model.ErrorMessage = $"No movie found for \"{model.SearchTerm}\".";
            }
            else
            {
                // Step 3b: Feed the successful search back to the view.
                model.MovieResult = movie;
            }
        }
        catch (Exception ex)
        {
            // Step 4: Surface unexpected errors while still rendering the page.
            model.ErrorMessage = $"Unable to search right now: {ex.Message}";
        }

        // Step 5: Refresh favorites so the UI reflects any additions the search service persisted.
        model.Favorites = (await _movieRepository.GetAllAsync(cancellationToken)).ToList();
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFavorite(string imdbId, CancellationToken cancellationToken)
    {
        // Step 1: Ignore requests without an identifier.
        if (!string.IsNullOrWhiteSpace(imdbId))
        {
            // Step 2: Delegate deletion to the repository (EF Core handles persistence).
            await _movieRepository.DeleteAsync(imdbId, cancellationToken);
        }

        // Step 3: Redirect back to the main page so the list refreshes.
        return RedirectToAction(nameof(Index));
    }
}
