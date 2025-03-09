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
            var current_time = DateTime.Now;
            var milisecond_until_next_minute = 60000 - ((current_time.Second * 1000) + current_time.Millisecond);
            await Task.Delay(milisecond_until_next_minute, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var concludePost = dbContext.Posts.Where(p => p.CloseDate <= DateTime.Now && p.Status == PostStatus.Open).ToList();
                    var announcements = new List<Announcement>();
                    var announcementRecipients = new List<AnnouncementRecipient>();
                    foreach (var post in concludePost)
                    {
                        post.Status = PostStatus.Concluded;
                        var announcement = new Announcement() 
                        {
                            PostId = post.Id,
                            Type = AnnouncementType.CreatorUpdate,
                            Message = $"Your post '{post.Title}' has been concluded and is now ready to announce the result!"
                        };
                        announcements.Add(announcement);
                    }
                    dbContext.AddRange(announcements);
                    dbContext.SaveChanges();
                    foreach (var announcement in announcements)
                    {
                        var post = concludePost.First(p => p.Id == announcement.PostId);
                        announcementRecipients.Add(new AnnouncementRecipient()
                        {
                            AnnouncementId = announcement.Id,
                            AccountId = post.CreatorId
                        });
                    }
                    dbContext.AddRange(announcementRecipients);
                    dbContext.SaveChanges();
                    Console.WriteLine(DateTime.Now);
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}