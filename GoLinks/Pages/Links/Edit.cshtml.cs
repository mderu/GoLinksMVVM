using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GoLinks.Data;
using GoLinks.Models.Links;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace GoLinks.Pages.Links
{
    public class EditModel : PageModel
    {
        private readonly GoLinkContext dbContext;

        public EditModel(GoLinkContext context)
        {
            dbContext = context;
        }

        [BindProperty]
        public GoLink GoLink { get; set; } = default!;

        /// <summary>
        /// The message to leave when an error occurs in the format of the input.
        /// </summary>
        [BindProperty]
        public string ErrorMessage { get; set; } = "";

        /// <summary>
        /// The message to leave when editing the link is successful.
        /// </summary>
        [BindProperty]
        public string SuccessMessage { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            ErrorMessage = "";
            SuccessMessage = "";

            if (id == null || dbContext.GoLinks == null)
            {
                ErrorMessage = "ID not provided.";
                return Page();
            }

            var goLink = await dbContext.GoLinks.FirstOrDefaultAsync(m => m.Id == id);
            if (goLink == null)
            {
                ErrorMessage = "Unable to find the given ID.";
                return Page();
            }
            GoLink = goLink;

            SuccessMessage = "";
            return Page();
        }

        private bool IsValid([NotNullWhen(false)] out string errorMessage)
        {
            if (GoLink.Owner is null)
            {
                errorMessage = $"{nameof(GoLink.Owner)} is missing from the link.";
                return false;
            }

            if (GoLink.ShortLink is null)
            {
                errorMessage = $"{nameof(GoLink.ShortLink)} is missing from the link.";
                return false;
            }

            string firstPart = GoLink.ShortLink.Split('/')[0];
            if (GoLink.InvalidShortLinkStartSegments.Contains(firstPart))
            {
                errorMessage = $"{GoLink.ShortLink} cannot start with \"{firstPart}/\"";
                return false;
            }

            if (GoLink.DestinationLink is null)
            {
                errorMessage = $"{nameof(GoLink.DestinationLink)} is missing from the link.";
                return false;
            }

            string cleanedDestinationLink = Regex.Replace(GoLink.DestinationLink, "\\^{[^{}]+}", "placeholder");

            if (!Uri.IsWellFormedUriString(cleanedDestinationLink, UriKind.RelativeOrAbsolute))
            {
                errorMessage = $"{GoLink.DestinationLink} must be URL encoded (aside from formatting strings).";
                return false;
            }

            // TODO: Check if the Owner actually exists (don't want a misspelling).

            errorMessage = "";
            return true;
        }

        public bool TryEditLink([NotNullWhen(true)] out GoLink? newLink)
        {
            if (!IsValid(out string errorMessage))
            {
                SuccessMessage = "";
                ErrorMessage = errorMessage;
                newLink = null;
                return false;
            }

            // The actual LinkToEdit originally passed into the model is not passed back and forth from the server
            // to client and back between the GET and POST calls. We'll need to fetch this link again.
            //
            // In a way, this limitation forces us to implement a feature that allows us to edit a link without
            // knowing the exact details of what the link's properties are.
            var goLinks = dbContext.GoLinks;

            var foo = goLinks
                .Select(link => link.Id).ToList();

            GoLink? LinkToEdit = goLinks
                .Where(link => link.Id == GoLink.Id)
                .FirstOrDefault();
            if (LinkToEdit is null)
            {
                SuccessMessage = "";
                ErrorMessage = $"Unable to find link with id \"{GoLink.Id}\".";
                newLink = null;
                return false;
            }

            // Forgivenesses: Validation above has already guaranteed these are not null.
            string modifications = (LinkToEdit.Owner != GoLink.Owner!)
                ? $" • Owner: {LinkToEdit.Owner} => {GoLink.Owner}\n" : "";
            modifications += (LinkToEdit.ShortLink != GoLink.ShortLink!)
                ? $" • Short Link: {LinkToEdit.ShortLink} => {GoLink.ShortLink}\n" : "";
            modifications += (LinkToEdit.DestinationLink != GoLink.DestinationLink!)
                ? $" • Destination Link: {LinkToEdit.DestinationLink} => {GoLink.DestinationLink}\n" : "";

            if (modifications.Length == 0)
            {
                SuccessMessage = "";
                ErrorMessage = "User input matches the link in the database. No changes were made.";
                newLink = null;
                return false;
            }

            LinkToEdit.Owner = GoLink.Owner!;
            LinkToEdit.ShortLink = GoLink.ShortLink!;
            LinkToEdit.DestinationLink = GoLink.DestinationLink!;
            // Effectively a no-op, but I want to call out here that the NumUses is never allowed to be set
            // by user input.
            LinkToEdit.NumUses = LinkToEdit.NumUses;

            SuccessMessage = $"Success! go/{LinkToEdit.ShortLink} has the following edits:\n {modifications.Trim()}.";
            ErrorMessage = "";
            newLink = LinkToEdit;
            return true;
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove(nameof(ErrorMessage));
            ModelState.Remove(nameof(SuccessMessage));

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (TryEditLink(out GoLink? editedGoLink))
            {
                dbContext.Attach(editedGoLink).State = EntityState.Modified;

                try
                {
                    await dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GoLinkExists(GoLink.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return Page();
        }

        private bool GoLinkExists(int id)
        {
            return (dbContext.GoLinks?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
