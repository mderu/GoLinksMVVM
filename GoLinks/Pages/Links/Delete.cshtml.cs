using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GoLinks.Data;
using GoLinks.Models.Links;

namespace GoLinks.Pages.Links
{
    public class DeleteModel : PageModel
    {
        private readonly GoLinks.Data.GoLinkContext _context;

        public DeleteModel(GoLinks.Data.GoLinkContext context)
        {
            _context = context;
        }

        [BindProperty]
      public GoLink GoLink { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.GoLinks == null)
            {
                return NotFound();
            }

            var golink = await _context.GoLinks.FirstOrDefaultAsync(m => m.Id == id);

            if (golink == null)
            {
                return NotFound();
            }
            else 
            {
                GoLink = golink;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.GoLinks == null)
            {
                return NotFound();
            }
            var golink = await _context.GoLinks.FindAsync(id);

            if (golink != null)
            {
                GoLink = golink;
                _context.GoLinks.Remove(GoLink);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
