using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace GoLinks.Models.Links
{
    [Index(nameof(Id), nameof(ShortLink), nameof(Owner), nameof(NumUses))]
    public class GoLink
    {
        /// <summary>
        /// The list of all invalid first segments for the ShortLink path.
        /// </summary>
        public static string[] InvalidShortLinkStartSegments = new string[] { "", "Links" };

        /// <summary>
        /// The primary key in the database.
        /// </summary>
        [Key]
        public int Id { get; set; }

        [BindProperty, Required]
        public string Owner { get; set; }

        [BindProperty, Required, DisplayName("Short Link Path")]
        public string ShortLink { get; set; }

        [BindProperty, Required, DisplayName("Destination Link")]
        public string DestinationLink { get; set; }

        [BindProperty]
        public int NumUses { get; set; } = 0;

        [Obsolete("Should only be called via reflection")]
        public GoLink() { }

        public GoLink(NewLinkRequest newRequest)
        {
            Owner = newRequest.Owner
                ?? throw new ArgumentException($"{nameof(newRequest.Owner)} not present");
            ShortLink = newRequest.ShortLink
                ?? throw new ArgumentException($"{nameof(newRequest.ShortLink)} not present");
            DestinationLink = newRequest.DestinationLink
                ?? throw new ArgumentException($"{nameof(newRequest.DestinationLink)} not present");
        }
    }
}
