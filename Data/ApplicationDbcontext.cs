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
            .HasForeignKey(e => e.AccountId);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Post)
            .WithMany(p => p.Enrollments)
            .HasForeignKey(e => e.PostId);

        modelBuilder.Entity<Post>()
            .HasOne(p => p.Creator)
            .WithMany(a => a.Posts)
            .HasForeignKey(p => p.CreaterId);
            // .OnDelete(DeleteBehavior.Cascade);
    }

    public DbSet<Post> Posts { get; set;}
    public DbSet<Account> Accounts { get; set;}
    public DbSet<Enrollment> Enrollments { get; set;}
}