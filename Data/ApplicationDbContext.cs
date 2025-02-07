using Microsoft.EntityFrameworkCore;
using StealAllTheCats.Models;
namespace StealAllTheCats.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<CatEntity> Cats { get; set; }
        public DbSet<TagEntity> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Define composite key for CatTag
    modelBuilder.Entity<CatTag>()
        .HasKey(ct => new { ct.CatEntityId, ct.TagEntityId });

    // Define relationships
    modelBuilder.Entity<CatTag>()
        .HasOne(ct => ct.CatEntity)
        .WithMany(c => c.CatTags)
        .HasForeignKey(ct => ct.CatEntityId)
        .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<CatTag>()
        .HasOne(ct => ct.TagEntity)
        .WithMany(t => t.CatTags)
        .HasForeignKey(ct => ct.TagEntityId)
        .OnDelete(DeleteBehavior.Cascade);
}

    }
}