using GoLinks.Models.Links;
using Microsoft.EntityFrameworkCore;

namespace GoLinks.Data
{
    public class GoLinkContext : DbContext
    {
        public GoLinkContext(DbContextOptions<GoLinkContext> options) : base(options)
        {
        }

        public DbSet<GoLink> GoLinks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GoLink>().ToTable("GoLinks");
        }
    }
}
