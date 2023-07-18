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
    public class DetailsModel : PageModel
    {
        private readonly GoLinks.Data.GoLinkContext _context;

        public DetailsModel(GoLinks.Data.GoLinkContext context)
        {
            _context = context;
        }

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
    }
}
