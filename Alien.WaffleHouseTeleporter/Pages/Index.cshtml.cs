using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Alien.WaffleHouseTeleporter.Pages
{

    public class IndexModel : PageModel
    {
        [BindProperty]
        public string ZipCode { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(ZipCode))
            {
                ModelState.AddModelError(string.Empty, "Enter a ZIP code to lock onto a Waffle House signal.");
                return Page();
            }

            return RedirectToPage("/Teleport", new { zip = ZipCode.Trim() });
        }
    }
}
