using Alien.WaffleHouseTeleporter.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace Alien.WaffleHouseTeleporter.Pages;
public class IndexModel(TeleporterService teleporter) : PageModel
{
    public int LocationCount => teleporter.Count;
    public bool HasEmbed => teleporter.HasEmbed;
    public void OnGet() { }
}
