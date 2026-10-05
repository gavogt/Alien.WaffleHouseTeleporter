using System.Globalization;
using System.Text.Json;
using Alien.WaffleHouseTeleporter.Models;

namespace Alien.WaffleHouseTeleporter.Services;

public sealed class TeleporterService
{
    private readonly LocationCatalog _catalog;
    private readonly string? _embedKey;

    public TeleporterService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        var path = Path.Combine(environment.ContentRootPath, "Data", "waffle-houses.json");
        _catalog = JsonSerializer.Deserialize<LocationCatalog>(File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("The Waffle House catalog could not be read.");
        if (_catalog.Locations.Count == 0 || _catalog.Locations.Any(x =>
            string.IsNullOrWhiteSpace(x.Id) || !double.IsFinite(x.Latitude) ||
            !double.IsFinite(x.Longitude) || x.Latitude is < 24 or > 50 ||
            x.Longitude is < -125 or > -66) ||
            _catalog.Locations.Select(x => x.Id).Distinct().Count() != _catalog.Locations.Count)
            throw new InvalidOperationException("The Waffle House catalog contains invalid destinations.");
        _embedKey = configuration["GoogleMaps:ApiKey"];
    }

    public int Count => _catalog.Locations.Count;
    public string RetrievedAt => _catalog.RetrievedAt;
    public bool HasEmbed => !string.IsNullOrWhiteSpace(_embedKey);

    public PortalDestination Pick(string? exclude = null)
    {
        var previous = _catalog.Locations.FindIndex(x => x.Id == exclude);
        if (previous < 0 || Count == 1)
            return _catalog.Locations[Random.Shared.Next(Count)];
        var index = Random.Shared.Next(Count - 1);
        return _catalog.Locations[index >= previous ? index + 1 : index];
    }

    public PortalDestination? Find(string id) => _catalog.Locations.Find(x => x.Id == id);
    private static string Coordinates(PortalDestination location) =>
        string.Create(CultureInfo.InvariantCulture, $"{location.Latitude},{location.Longitude}");

    public object Describe(PortalDestination location) => new
    {
        location.Id, location.Name, location.Address,
        coordinates = Coordinates(location),
        streetViewUrl = StreetViewUrl(location), mapsUrl = MapsUrl(location),
        embedUrl = EmbedUrl(location),
        detailsUrl = $"/Teleport?id={Uri.EscapeDataString(location.Id)}"
    };

    public string StreetViewUrl(PortalDestination location) =>
        $"https://www.google.com/maps/@?api=1&map_action=pano&viewpoint={Uri.EscapeDataString(Coordinates(location))}";

    public string MapsUrl(PortalDestination location) =>
        $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString($"Waffle House {Coordinates(location)}")}";

    public string? EmbedUrl(PortalDestination location) => HasEmbed
        ? $"https://www.google.com/maps/embed/v1/streetview?key={Uri.EscapeDataString(_embedKey!)}&location={Uri.EscapeDataString(Coordinates(location))}"
        : null;
}
