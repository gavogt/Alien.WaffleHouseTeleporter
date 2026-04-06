using Alien.WaffleHouseTeleporter.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WaffleHouseTeleporter.Services;

namespace Alien.WaffleHouseTeleporter.Pages;

public class TeleportModel : PageModel
{
    private readonly GoogleMapsService _googleMapsService;

    public TeleportModel(GoogleMapsService googleMapsService)
    {
        _googleMapsService = googleMapsService;
    }

    [BindProperty(SupportsGet = true)]
    public string Zip { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? PlaceId { get; set; }

    public string SearchCenterLabel { get; set; } = string.Empty;
    public List<WaffleHouseLocation> Locations { get; set; } = new();
    public WaffleHouseLocation? SelectedLocation { get; set; }
    public string? StreetViewUrl { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Zip))
        {
            return RedirectToPage("/Index");
        }

        var coordinates = await _googleMapsService.GeocodeZipAsync(Zip, cancellationToken);
        if (coordinates is null)
        {
            ErrorMessage = "Unable to resolve that ZIP code. The teleporter lost the breadcrumb trail.";
            return Page();
        }

        SearchCenterLabel = coordinates.FormattedAddress;
        Locations = await _googleMapsService.FindNearbyWaffleHousesAsync(
            coordinates.Latitude,
            coordinates.Longitude,
            cancellationToken);

        if (Locations.Count == 0)
        {
            ErrorMessage = "No Waffle House signal detected in this region.";
            return Page();
        }

        SelectedLocation = !string.IsNullOrWhiteSpace(PlaceId)
            ? Locations.FirstOrDefault(x => x.PlaceId == PlaceId)
            : Locations.First();

        if (SelectedLocation is not null)
        {
            StreetViewUrl = _googleMapsService.BuildStreetViewEmbedUrl(
                SelectedLocation.Latitude,
                SelectedLocation.Longitude);
        }

        return Page();
    }
}