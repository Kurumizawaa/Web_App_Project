using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_app_project.Data;
using web_app_project.Models;

namespace web_app_project.Controllers;

public class AnnouncementController : Controller
{
    private readonly ApplicationDbContext _Dbcontext;

    public AnnouncementController(ApplicationDbContext dbcontext)
    {
        _Dbcontext = dbcontext;
    }

    public JsonResult GetAnnouncement()
    {
        var Id = HttpContext.Session.GetInt32("ID");
        if (Id != null)
        {
            var announcements = _Dbcontext.AnnouncementRecipients.Include(a => a.Announcement).ThenInclude(b => b!.Post).ThenInclude(c => c!.Creator).Where(x => x.AccountId == Id).Select(y => y.Announcement).OrderByDescending(z => z!.AnnounceAt).ToList();
            List<object> announcement_list = new List<object>();
            foreach (var announcement in announcements)
            {
                announcement_list.Add( new {type = announcement!.Type, message = announcement.Message, time = announcement.AnnounceAt, postid = announcement.PostId, title = announcement.Post?.Title ?? null, creator = announcement.Post?.Creator!.Username ?? null});
            }

            return Json( new { getannouncement_successful = true, announcement_list });
        }
        else
        {
            TempData["Info"] = "Your session id has been expired! Login again to continue.";
            return Json( new { getannouncement_successful = false, message = "You might wanna login first bro." });
        }
    }

    public IActionResult PostResultAnnouncement(int postid)
    {
        var accountid = HttpContext.Session.GetInt32("ID");
        var post = _Dbcontext.Posts.FirstOrDefault(x => x.Id == postid);
        if (accountid != null)
        {
            if (post != null && post.Status == PostStatus.Selected && accountid == post.CreatorId)
            {
                return View(post);
            }
            else
            {
                //handle this later
                return RedirectToAction("Account","Account");
            }
        }
        else
        {
            TempData["Info"] = "Your session id has been expired! Login again to continue.";
            return RedirectToAction("Login","Account");
        }
    }

    [HttpPost]
    public JsonResult PostResultAnnouncement(int postid, Announcement announce_selected, Announcement announce_rejected)
    {
        var accountid = HttpContext.Session.GetInt32("ID");
        if (accountid != null)
        {
            if (ModelState.IsValid)
            {
                var post = _Dbcontext.Posts.Include(a => a.SelectedAccounts).Include(b => b.Enrollments)!.ThenInclude(c => c.Account).FirstOrDefault(x => x.Id == postid);
                if (post != null && post.Status == PostStatus.Selected && accountid == post.CreatorId)
                {
                    post.Status = PostStatus.Archived;
                    announce_selected.PostId = post.Id;
                    announce_rejected.PostId = post.Id;
                    _Dbcontext.Announcements.AddRange(announce_selected, announce_rejected);
                    _Dbcontext.SaveChanges();
                    List<AnnouncementRecipient> announcement_recipient = new List<AnnouncementRecipient>();
                    foreach (var account in post.SelectedAccounts!)
                    {
                        announcement_recipient.Add(new AnnouncementRecipient(){AnnouncementId = announce_selected.Id, AccountId = account!.Id});
                    }
                    foreach (var account in post.Enrollments!.Select(x => x.Account).Except(post.SelectedAccounts).ToList())
                    {
                        announcement_recipient.Add(new AnnouncementRecipient(){AnnouncementId = announce_rejected.Id, AccountId = account!.Id});
                    }
                    _Dbcontext.AnnouncementRecipients.AddRange(announcement_recipient);
                    _Dbcontext.SaveChanges();
                    return Json(new { announce_successful = true });
                }
                else
                {
                    //handle this later
                    return Json(null);
                }
            }
            else
            {
                //handle this later
                return Json(null);
            }
        }
        else
        {
            //handle this later
            TempData["Info"] = "Your session id has been expired! Login again to continue.";
            return Json(null);
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
