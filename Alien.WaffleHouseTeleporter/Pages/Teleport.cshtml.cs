using Alien.WaffleHouseTeleporter.Models;
using Alien.WaffleHouseTeleporter.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Alien.WaffleHouseTeleporter.Pages;
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class TeleportModel(TeleporterService teleporter) : PageModel
{
    public PortalDestination SelectedLocation { get; private set; } = null!;
    public string StreetViewUrl => teleporter.StreetViewUrl(SelectedLocation);
    public string MapsUrl => teleporter.MapsUrl(SelectedLocation);
    public string? EmbedUrl => teleporter.EmbedUrl(SelectedLocation);
    public IActionResult OnGet(string? id, string? exclude)
    {
        var destination = id is null ? teleporter.Pick(exclude) : teleporter.Find(id);
        if (destination is null) return NotFound();
        SelectedLocation = destination;
        return Page();
    }
    public IActionResult OnGetRandom(string? exclude) =>
        new JsonResult(teleporter.Describe(teleporter.Pick(exclude)));
    public IActionResult OnGetLaunch(string? exclude)
    {
        var destination = teleporter.Pick(exclude);
        return teleporter.HasEmbed
            ? RedirectToPage("/Teleport", new { id = destination.Id })
            : Redirect(teleporter.StreetViewUrl(destination));
    }
}
