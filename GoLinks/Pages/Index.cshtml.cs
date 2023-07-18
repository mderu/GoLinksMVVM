using GoLinks.Data;
using GoLinks.Models.Links;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.RegularExpressions;
using System.Web;

namespace GoLinks.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly GoLinkContext dbContext;

        public IndexModel(GoLinkContext context, ILogger<IndexModel> logger)
        {
            dbContext = context;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            string fullPath = HttpContext.Request.Path.Value?[1..] ?? "";
            // Strip the leading "/".
            fullPath = fullPath.TrimStart('/');

            if (!ModelState.IsValid || dbContext.GoLinks == null)
            {
                // TODO: Return an error here.
                return Page();
            }

            if (fullPath == "")
            {
                return Page();
            }

            GoLink? link;
            link = dbContext.GoLinks.FirstOrDefault(l => l.ShortLink == fullPath);

            if (link is null)
            {
                int lastSlash = fullPath.LastIndexOf('/');
                lastSlash = lastSlash < 0 ? fullPath.Length : lastSlash;
                string prefix = fullPath[..lastSlash];

                string newUrl = QueryHelpers.AddQueryString("/Links/Index", nameof(GoLink.ShortLink), prefix);
                return Redirect(newUrl);
            }

            link.NumUses += 1;
            dbContext.GoLinks.Update(link);
            dbContext.SaveChanges();

            string queryString = HttpContext.Request.QueryString.Value ?? "";

            return Redirect(ApplyFormatting(link.DestinationLink, queryString));
        }

        private string ApplyFormatting(string destinationUrl, string queryString)
        {
            if (string.IsNullOrEmpty(queryString))
            {
                return destinationUrl;
            }
            string[] paramArray = queryString.Length > 0
                ? queryString[1..].Split('&')
                : Array.Empty<string>();
            var paramKvps = HttpUtility.ParseQueryString(queryString);

            bool hasFormattingChange = false;
            while (true)
            {
                var match = Regex.Match(destinationUrl, "\\^{(?<Query>[^{}]+)}");
                if (!match.Success)
                {
                    break;
                }
                hasFormattingChange = true;
                string query = match.Groups["Query"].Value;
                string replacement;
                if (query == "*")
                {
                    replacement = queryString.Trim('?');
                }
                else if (int.TryParse(query, out int result))
                {
                    try
                    {
                        replacement = paramArray[result];
                    }
                    catch (Exception)
                    {
                        replacement = "";
                    }
                }
                else
                {
                    try
                    {
                        replacement = paramKvps[query] ?? "";
                    }
                    catch (Exception)
                    {
                        replacement = "";
                    }
                }
                destinationUrl = destinationUrl.Replace(
                    match.Groups[0].Value,
                    replacement,
                    comparisonType: StringComparison.OrdinalIgnoreCase);
            }
            if (!hasFormattingChange)
            {
                destinationUrl += queryString;
            }
            return destinationUrl;
        }
    }
}