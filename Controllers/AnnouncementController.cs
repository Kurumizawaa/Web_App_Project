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
            var announcements = _Dbcontext.AnnouncementRecipients.Include(a => a.Announcement).ThenInclude(b => b!.Post).ThenInclude(c => c!.Creator).Where(x => x.AccountId == Id).Select(y => new {y.Announcement, y.IsRead}).OrderByDescending(z => z!.Announcement!.AnnounceAt).ToList();
            List<object> announcement_list = new List<object>();
            foreach (var announcement in announcements)
            {
                announcement_list.Add( new {id = announcement.Announcement!.Id, type = announcement!.Announcement!.Type, message = announcement.Announcement.Message, time = announcement.Announcement.AnnounceAt, isread = announcement.IsRead, postid = announcement.Announcement.PostId, title = announcement.Announcement.Post?.Title ?? null, creator = announcement.Announcement.Post?.Creator!.Username ?? null, picture = announcement.Announcement.Post?.Picture ?? null});
            }

            return Json( new { getannouncement_successful = true, announcement_list });
        }
        else
        {
            return Json( new { getannouncement_successful = false, message = "You might wanna login first bro." });
        }
    }

    public JsonResult AckAnnouncement(int announcementid)
    {
        var accountid = HttpContext.Session.GetInt32("ID");
        if (accountid != null)
        {
            var announcement_recipient = _Dbcontext.AnnouncementRecipients.FirstOrDefault(x => x.AnnouncementId == announcementid && x.AccountId == accountid);
            if (announcement_recipient != null)
            {
                announcement_recipient.IsRead = true;
                _Dbcontext.SaveChanges();
                return Json(new { read_successfully = true });
            }
            else
            {
                return Json(new { read_successfully = false });
            }
        }
        else
        {
            return Json(new { read_successfully = false });
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
                return PartialView("_PostResultAnnouncement", post);
            }
            else
            {
                return Unauthorized();
            }
        }
        else
        {
            return Unauthorized();
        }
    }

    public IActionResult GeneralAnnouncement(int? postid)
    {
        var accountid = HttpContext.Session.GetInt32("ID");
        if (accountid != null)
        {
            if (postid != null)
            {
                var post = _Dbcontext.Posts.FirstOrDefault(a => a.Id == postid);
                if (post != null && post.Status != PostStatus.Archived && post.CreatorId == accountid)
                {
                    return PartialView("_GeneralAnnouncement", post);
                }
                else
                {
                    return Unauthorized();
                }
            }
            else
            {
                return PartialView("_GeneralAnnouncement", new Post());
            }
        }
        else
        {
            return Unauthorized();
        }
    }

    public JsonResult GetRelatedAnnouncement(int postid)
    {
        var accountid = HttpContext.Session.GetInt32("ID");
        if (accountid != null)
        {
            var account = _Dbcontext.Accounts.Include(a => a.Announcements)!.ThenInclude(b => b.Announcement).FirstOrDefault(x => x.Id == accountid);
            var post = _Dbcontext.Posts.Include(a => a.Announcements).FirstOrDefault(x => x.Id == postid);
            if (post != null)
            {
                var globally_announce = post.Announcements!.Where(x => x.Type == AnnouncementType.CreatorUpdate || x.Type == AnnouncementType.General).ToList();
                var locally_announce = account!.Announcements!.Where(x => x.Announcement!.PostId == postid).Select(y => y.Announcement!).Except(globally_announce).ToList();
                var related_announcement = globally_announce.Union(locally_announce).Select(a => new { a.Message, a.Type, a.AnnounceAt }).OrderByDescending(x => x.AnnounceAt).ToList();
                return Json(new { get_successfully = true, related_announcement });
            }
            else
            {
                return Json(new { get_successfully = false });
            }
        }
        else
        {
            return Json(new { get_successfully = false });
        }
    }

    [HttpPost]
    public IActionResult PostResultAnnouncement(int postid, Announcement announce_selected, Announcement announce_rejected)
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
                    var creator_update_announcement = new Announcement()
                    {
                        PostId = post.Id,
                        Type = AnnouncementType.CreatorUpdate,
                        Message = $"Your post '{post.Title}' has successfully announced the result and is now kept in the archive!"
                    };
                    _Dbcontext.Announcements.AddRange(announce_selected, announce_rejected, creator_update_announcement);
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
                    announcement_recipient.Add(new AnnouncementRecipient(){AnnouncementId = creator_update_announcement.Id, AccountId = post.CreatorId});
                    _Dbcontext.AnnouncementRecipients.AddRange(announcement_recipient);
                    _Dbcontext.SaveChanges();
                    TempData["snackbar-message"] = "announcing result successfully";
                    TempData["snackbar-type"] = "success";
                    return RedirectToAction("Post","Post", new { id = post.Id });
                }
                else
                {
                    return Unauthorized();
                }
            }
            else
            {
                return RedirectToAction("PostResultAnnouncement","Announcement", new { postid });
            }
        }
        else
        {
            TempData["snackbar-message"] = "Your session id has been expired! Login again to continue.";
            TempData["snackbar-type"] = "error";
            return RedirectToAction("Login","Account");
        }
    }

    [HttpPost]
    public IActionResult GeneralAnnouncement(int? postid, Announcement announcement, int[] recipientid_list)
    {
        var accountid = HttpContext.Session.GetInt32("ID");
        if (accountid != null)
        {
            if (ModelState.IsValid)
            {
                announcement.PostId = (postid != 0) ? postid : null;
                _Dbcontext.Announcements.Add(announcement);
                _Dbcontext.SaveChanges();
                List<AnnouncementRecipient> announcement_recipient = new List<AnnouncementRecipient>();
                if (postid != null && postid != 0)
                {
                    var post = _Dbcontext.Posts.Include(a => a.Enrollments)!.ThenInclude(b => b.Account).FirstOrDefault(p => p.Id == postid);
                    if (post != null && post.Status != PostStatus.Archived)
                    {
                        foreach (var account in post.Enrollments!.Select(a => a.Account).ToList())
                        {
                            announcement_recipient.Add(new AnnouncementRecipient(){ AnnouncementId = announcement.Id, AccountId = account!.Id,});
                        }
                        _Dbcontext.AnnouncementRecipients.AddRange(announcement_recipient);
                        _Dbcontext.SaveChanges();
                        TempData["snackbar-message"] = "announcing successfully";
                        TempData["snackbar-type"] = "success";
                        return RedirectToAction("Post","Post", new { id = post.Id });
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }
                else
                {
                    foreach (var recipientid in recipientid_list)
                    {
                        announcement_recipient.Add(new AnnouncementRecipient(){ AnnouncementId = announcement.Id, AccountId = recipientid,});
                    }
                    _Dbcontext.AnnouncementRecipients.AddRange(announcement_recipient);
                    _Dbcontext.SaveChanges();
                    TempData["snackbar-message"] = "announcing successfully";
                    TempData["snackbar-type"] = "success";
                    return RedirectToAction("Index","Home");
                }
            }
            else
            {
                return RedirectToAction("GeneralAnnouncement","Announcement", new { postid });
            }
        }
        else
        {
            TempData["snackbar-message"] = "Your session id has been expired! Login again to continue.";
            TempData["snackbar-type"] = "error";
            return RedirectToAction("Login","Account");
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
