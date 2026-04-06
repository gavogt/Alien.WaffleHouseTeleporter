using Alien.WaffleHouseTeleporter.Models;
using Alien.WaffleHouseTeleporter.Options;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace WaffleHouseTeleporter.Services;

public sealed class GoogleMapsService
{
    private readonly HttpClient _httpClient;
    private readonly GoogleMapsOptions _options;

    public GoogleMapsService(HttpClient httpClient, IOptions<GoogleMapsOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<CoordinateResult?> GeocodeZipAsync(string zipCode, CancellationToken cancellationToken = default)
    {
        var encodedZip = WebUtility.UrlEncode(zipCode);
        var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={encodedZip}&key={_options.ApiKey}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var root = document.RootElement;
        var status = root.GetProperty("status").GetString();
        if (!string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var result = root.GetProperty("results")[0];
        var location = result.GetProperty("geometry").GetProperty("location");

        return new CoordinateResult
        {
            Latitude = location.GetProperty("lat").GetDouble(),
            Longitude = location.GetProperty("lng").GetDouble(),
            FormattedAddress = result.GetProperty("formatted_address").GetString() ?? zipCode
        };
    }

    public async Task<List<WaffleHouseLocation>> FindNearbyWaffleHousesAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        var location = string.Create(CultureInfo.InvariantCulture, $"{latitude},{longitude}");
        var query = WebUtility.UrlEncode("Waffle House");

        var url = "https://maps.googleapis.com/maps/api/place/nearbysearch/json"
                + $"?location={location}"
                + "&radius=50000"
                + $"&keyword={query}"
                + $"&key={_options.ApiKey}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var root = document.RootElement;
        var status = root.GetProperty("status").GetString();
        if (!string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(status, "ZERO_RESULTS", StringComparison.OrdinalIgnoreCase))
        {
            return new List<WaffleHouseLocation>();
        }

        var locations = new List<WaffleHouseLocation>();

        if (!root.TryGetProperty("results", out var results))
        {
            return locations;
        }

        foreach (var item in results.EnumerateArray())
        {
            var geo = item.GetProperty("geometry").GetProperty("location");
            var itemLat = geo.GetProperty("lat").GetDouble();
            var itemLng = geo.GetProperty("lng").GetDouble();

            var address = item.TryGetProperty("vicinity", out var vicinity)
                ? vicinity.GetString()
                : item.TryGetProperty("formatted_address", out var formattedAddress)
                    ? formattedAddress.GetString()
                    : "Unknown address";

            locations.Add(new WaffleHouseLocation
            {
                Name = item.GetProperty("name").GetString() ?? "Waffle House",
                Address = address ?? "Unknown address",
                PlaceId = item.GetProperty("place_id").GetString() ?? string.Empty,
                Latitude = itemLat,
                Longitude = itemLng,
                DistanceMiles = CalculateDistanceMiles(latitude, longitude, itemLat, itemLng)
            });
        }

        return locations
            .OrderBy(x => x.DistanceMiles)
            .Take(8)
            .ToList();
    }

    public string BuildStreetViewEmbedUrl(double latitude, double longitude)
    {
        return "https://www.google.com/maps/embed/v1/streetview"
             + $"?key={_options.ApiKey}"
             + string.Create(CultureInfo.InvariantCulture, $"&location={latitude},{longitude}")
             + "&heading=210&pitch=5&fov=80";
    }

    private static double CalculateDistanceMiles(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusMiles = 3958.8;

        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Pow(Math.Sin(dLat / 2), 2)
              + Math.Cos(DegreesToRadians(lat1))
              * Math.Cos(DegreesToRadians(lat2))
              * Math.Pow(Math.Sin(dLon / 2), 2);

        var c = 2 * Math.Asin(Math.Sqrt(a));
        return earthRadiusMiles * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
}
