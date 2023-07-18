using Microsoft.AspNetCore.Mvc;
using GoLinks.Data;

namespace GoLinks.Models.Links
{
    public class NewLinkResponse
    {
        [BindProperty]
        public NewLinkRequest LinkRequest { get; set; }

        [BindProperty]
        public string? SuccessMessage { get; set; }

        [BindProperty]
        public string? ErrorMessage { get; set; }

        public NewLinkResponse()
        {
            LinkRequest = new NewLinkRequest();
            SuccessMessage = null;
            ErrorMessage = null;
        }

        public void OnPost(GoLinkContext dbContext)
        {
            if (!LinkRequest.IsValid(out string errorMessage))
            {
                ErrorMessage = errorMessage;
                SuccessMessage = null;
                return;
            }

            var goLinks = dbContext.GoLinks;

            var existingLink = goLinks
                .Where(link => link.ShortLink == LinkRequest.ShortLink)
                .FirstOrDefault();

            if (!(existingLink is null))
            {
                ErrorMessage =
                    $"Go link <a href=\"/Links/Edit?{existingLink.Id}\">go/{existingLink.ShortLink}</a> " +
                    $"already exists.";
                SuccessMessage = null;
                return;
            }

            goLinks.Add(new GoLink(LinkRequest));
            dbContext.SaveChanges();

            SuccessMessage = $"Success! go/{LinkRequest.ShortLink} now points to {LinkRequest.DestinationLink}.";
            ErrorMessage = null;
        }
    }
}
