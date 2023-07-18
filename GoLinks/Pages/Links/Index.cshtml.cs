using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GoLinks.Data;
using GoLinks.Models.Links;
using System.Text.RegularExpressions;

namespace GoLinks.Pages.Links
{
    public class IndexModel : PageModel
    {
        private readonly GoLinks.Data.GoLinkContext _context;

        public IndexModel(GoLinks.Data.GoLinkContext context)
        {
            _context = context;
        }

        public IList<GoLink> GoLinks { get;set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.GoLinks is null)
            {
                throw new Exception("Database context is not configured.");
            }

            if (HttpContext.Request.Query.TryGetValue(nameof(GoLink.Owner), out var ownerQuery))
            {
                // Assume a single name or part of a name was given.
                string nameRegex = $"%{ownerQuery}%";

                GoLinks = await _context
                    .GoLinks
                    .Where(link => EF.Functions.Like(link.Owner, nameRegex))
                    .ToListAsync();
            }
            else if (HttpContext.Request.Query.TryGetValue(nameof(GoLink.ShortLink), out var linkQuery))
            {
                // Assume a single name or part of a name was given.
                string nameRegex = $"%{linkQuery}%";

                GoLinks = await _context
                    .GoLinks
                    .Where(link => EF.Functions.Like(link.ShortLink, nameRegex))
                    .ToListAsync();
            }
            else
            {
                GoLinks = await _context.GoLinks.ToListAsync();
            }
        }

        private bool ContainsAny(IEnumerable<string> values, string trueValue)
        {
            foreach (var value in values)
            {
                if (trueValue.Contains(value))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
