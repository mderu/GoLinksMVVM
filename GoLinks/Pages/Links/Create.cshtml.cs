using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GoLinks.Data;
using GoLinks.Models.Links;

namespace GoLinks.Pages.Links
{
    public class CreateModel : PageModel
    {
        private readonly GoLinkContext _context;

        public CreateModel(GoLinkContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public GoLink GoLink { get; set; } = default!;


        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove($"{nameof(GoLink)}.{nameof(GoLink.NumUses)}");
            
            // Ensure the number of uses is always 0 at the start.
            GoLink.NumUses = 0;

            if (!ModelState.IsValid || _context.GoLinks == null || GoLink == null)
            {
                return Page();
            }

            _context.GoLinks.Add(GoLink);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
