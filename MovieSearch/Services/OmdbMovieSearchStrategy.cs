using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MovieSearch.Models;
using MovieSearch.Options;

namespace MovieSearch.Services;

public class OmdbMovieSearchStrategy : IMovieSearchStrategy
{
    private readonly HttpClient _httpClient;
    private readonly OmdbOptions _options;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public OmdbMovieSearchStrategy(HttpClient httpClient, IOptionsMonitor<OmdbOptions> optionsMonitor)
    {
        _httpClient = httpClient;
        _options = optionsMonitor.CurrentValue;
    }

    public async Task<Movie?> SearchAsync(string title, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("OMDb API key is not configured. Set Omdb:ApiKey or the OMDB_API_KEY environment variable.");
        }

        // Step 1: Build the OMDb URL using the configured API key and plot length preference.
        var plotParam = _options.PlotLength > 200 ? "full" : "short";
        var url = $"?t={Uri.EscapeDataString(title)}&apikey={_options.ApiKey}&plot={plotParam}";

        // Step 2: Execute the HTTP call (exceptions bubble up to the caller).
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        // Step 3: Deserialize the JSON payload into the OmdbResponse DTO.
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<OmdbResponse>(stream, _serializerOptions, cancellationToken);

        if (payload is null || !string.Equals(payload.Response, "True", StringComparison.OrdinalIgnoreCase))
        {
            return null; // Step 4: Return null when OMDb cannot find the title.
        }

        // Step 5: Map the DTO into the domain model consumed by the rest of the app.
        return new Movie
        {
            ImdbID = payload.ImdbID,
            Title = payload.Title,
            Year = payload.Year,
            Director = payload.Director,
            Plot = payload.Plot,
            Poster = payload.Poster,
            ImdbRating = payload.ImdbRating,
            RetrievedAtUtc = DateTime.UtcNow
        };
    }
}
