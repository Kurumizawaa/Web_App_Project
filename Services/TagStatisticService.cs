using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using web_app_project.Data;
using web_app_project.Models;

namespace web_app_project.Services;

public class TagStatisticService
{
    private readonly ApplicationDbContext _Dbcontext;
    private readonly IMemoryCache _MemoryCache;
    private readonly string toptagcachekey = "top4tag";

    public TagStatisticService(ApplicationDbContext dbcontext, IMemoryCache memoryCache)
    {
        _Dbcontext = dbcontext;
        _MemoryCache = memoryCache;
    }

    public List<Tag> GetTop4Tags()
    {
        if (!_MemoryCache.TryGetValue(toptagcachekey, out List<Tag>? top4tag))
        {
            top4tag = CalculateTop4Tags();
            _MemoryCache.Set(toptagcachekey, top4tag, new MemoryCacheEntryOptions() {AbsoluteExpiration = DateTimeOffset.MaxValue});
        }
        return top4tag!;
    }

    public void UpdateCache()
    {
        var top4tag = CalculateTop4Tags();
        _MemoryCache.Set(toptagcachekey, top4tag, new MemoryCacheEntryOptions() {AbsoluteExpiration = DateTimeOffset.MaxValue});
    }

    private List<Tag> CalculateTop4Tags() 
    {
        var top4tag = _Dbcontext.Tags.Include(a => a.Posts).OrderByDescending(b => b.Posts.Count).Take(4).Select(x => new Tag() {Name = x.Name}).ToList();
        return top4tag;
    }
}