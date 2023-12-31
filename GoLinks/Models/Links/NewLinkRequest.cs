﻿using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace GoLinks.Models.Links
{
    public class NewLinkRequest
    {
        /// <summary>
        /// A regex that matches the scheme portion of a URL. Note that we allow for more than just
        /// http:// and https://, which gives us the flexibility to link to applications opened on
        /// the desktop, such as Slack or other shared custom schemes.
        /// </summary>
        /// <remarks>
        /// See https://www.ietf.org/rfc/rfc1738.txt#:~:text=The%20main%20parts%20of%20URLs.
        /// </remarks>
        private readonly Regex schemeRegex = new Regex(@"^[a-z\+\.-]+://");

        // TODO: Have a better lookup for this elsewhere.
        private static readonly HashSet<string> InvalidNames = new()
        {
            // Home Page
            "",
            // The pages used to modify or view links
            "Links",
        };

        [BindProperty, Required]
        public string? Owner { get; set; }

        [BindProperty, Required, DisplayName("Short Link Path")]
        public string? ShortLink { get; set; }


        private string? destinationLink;

        [BindProperty, Required, DisplayName("Destination Link")]
        public string? DestinationLink
        {
            get
            {
                return destinationLink;
            }
            set
            {
                destinationLink = value;
                // Add http:// by default if users forget to put it there. If it's https,
                // they can always go back later and edit it.
                if (!(destinationLink is null) && !schemeRegex.IsMatch(destinationLink))
                {
                    destinationLink = $"http://{value}";
                    return;
                }
                destinationLink = value;
            }
        }

        public NewLinkRequest()
        {
        }

        public NewLinkRequest(GoLink goLink)
        {
            Owner = goLink.Owner;
            ShortLink = goLink.ShortLink;
            DestinationLink = goLink.DestinationLink;
        }

        public bool IsValid([NotNullWhen(false)] out string errorMessage)
        {
            if (Owner is null)
            {
                errorMessage = $"{nameof(Owner)} is missing from the link.";
                return false;
            }

            if (ShortLink is null)
            {
                errorMessage = $"{nameof(ShortLink)} is missing from the link.";
                return false;
            }

            string firstPart = ShortLink.Split('/')[0];
            if (InvalidNames.Contains(firstPart))
            {
                errorMessage = $"{ShortLink} cannot start with \"{firstPart}/\"";
                return false;
            }

            if (DestinationLink is null)
            {
                errorMessage = $"{nameof(DestinationLink)} is missing from the link.";
                return false;
            }

            string cleanedDestinationLink = Regex.Replace(DestinationLink, "\\^{[^{}]+}", "placeholder");

            if (!Uri.IsWellFormedUriString(cleanedDestinationLink, UriKind.RelativeOrAbsolute))
            {
                errorMessage = $"{DestinationLink} must be URL encoded (aside from formatting strings).";
                return false;
            }

            // TODO: Check if the Owner actually exists (don't want a misspelling).

            errorMessage = "";
            return true;
        }
    }
}
