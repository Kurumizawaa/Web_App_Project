using Microsoft.EntityFrameworkCore;
using web_app_project.Models;

namespace web_app_project.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> option) : base(option)
    {
        
    }
    public DbSet<Post> Post { get; set;}
}