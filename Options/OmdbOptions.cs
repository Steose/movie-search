namespace MovieSearch.Options;

public class OmdbOptions
{
    public const string SectionName = "Omdb";
    public string BaseAddress { get; set; } = "https://www.omdbapi.com/";
    public string ApiKey { get; set; } = string.Empty;
    public int PlotLength { get; set; } = 200;
}
