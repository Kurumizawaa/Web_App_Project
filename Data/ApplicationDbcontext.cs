using Microsoft.EntityFrameworkCore;
using web_app_project.Models;

namespace web_app_project.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> option) : base(option)
    {
        
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Enrollment>()
            .HasKey(e => new { e.AccountId, e.PostId });

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Account)
            .WithMany(a => a.Enrollments)
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Post)
            .WithMany(p => p.Enrollments)
            .HasForeignKey(e => e.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Post>()
            .HasOne(p => p.Creator)
            .WithMany(a => a.Posts)
            .HasForeignKey(p => p.CreatorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Post>()
            .HasMany(p => p.Tags)
            .WithMany(t => t.Posts)
            .UsingEntity<Dictionary<string, object>>(
                "PostTag",
                j => j.HasOne<Tag>().WithMany().HasForeignKey("TagId"),
                j => j.HasOne<Post>().WithMany().HasForeignKey("PostId")
            );

        modelBuilder.Entity<Post>()
            .HasMany(p => p.SelectedAccounts)
            .WithMany(a => a.SelectedInPosts)
            .UsingEntity<Dictionary<string, object>>(
                "PostAccountSelection",
                j => j.HasOne<Account>().WithMany().HasForeignKey("AccountId"),
                j => j.HasOne<Post>().WithMany().HasForeignKey("PostId")
            );

        modelBuilder.Entity<Announcement>()
            .HasKey(e => e.Id);

        modelBuilder.Entity<Announcement>()
            .HasOne(a => a.Post)
            .WithMany(p => p.Announcements)
            .HasForeignKey("PostId")
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Announcement>()
            .HasMany(p => p.Recipients)
            .WithMany(a => a.Announcements)
            .UsingEntity<Dictionary<string, object>>(
                "AccountAnnouncementRecipient",
                j => j.HasOne<Account>().WithMany().HasForeignKey("AccountId"),
                j => j.HasOne<Announcement>().WithMany().HasForeignKey("AnnouncementId")
            );
    }

    public DbSet<Post> Posts { get; set;}
    public DbSet<Account> Accounts { get; set;}
    public DbSet<Enrollment> Enrollments { get; set;}
    public DbSet<Tag> Tags { get; set;}
    public DbSet<Announcement> Announcements { get; set;}
}