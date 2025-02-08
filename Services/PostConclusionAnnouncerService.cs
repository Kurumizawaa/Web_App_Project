using Microsoft.EntityFrameworkCore;
using web_app_project.Data;
using web_app_project.Models;

public class PostConclusionAnnouncerService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    public PostConclusionAnnouncerService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var concludePost = dbContext.Posts.Where(p => p.CloseDate <= DateTime.Now && p.Status == PostStatus.Open).Include(x => x.Creator).ToList();
                foreach (var post in concludePost)
                {
                    post.Status = PostStatus.Conclude;
                    var announcement = new Announcement() 
                    {
                        PostId = post.Id,
                        Type = AnnouncementType.CreatorUpdate,
                        Message = $"Your post '{post.Title}' has been concluded and is now ready to announce the result!"
                    };
                    announcement.Recipients!.Add(post.Creator!);
                    dbContext.Announcements.Add(announcement);
                }
                dbContext.SaveChanges();
                Console.WriteLine(DateTime.Now);
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}