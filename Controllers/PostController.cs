using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using web_app_project.Data;
using web_app_project.Models;
using web_app_project.Services;

namespace web_app_project.Controllers;

public class PostController : Controller
{
    private readonly ApplicationDbContext _Dbcontext;
    private readonly TagStatisticService _TagStatisticService;

    public PostController(ApplicationDbContext dbcontext, TagStatisticService tagStatisticService)
    {
        _Dbcontext = dbcontext;
        _TagStatisticService = tagStatisticService;
    }
    
    public JsonResult GetPost(int page, string textquery, string tagquery, string typequery, int[] statusquery, string orderby, bool ascending)
    {
        var page_size = 4;
        var textquery_list = (textquery != null) ? textquery.ToLower().Split() : [];
        var tagquery_list = (tagquery != null) ? tagquery.ToLower().Split() : [];
        var typequery_list = (typequery != null) ? typequery.Split() : ["OnFull","OnSelect","OnRandom"];
        var statusquery_list = (statusquery.Length != 0) ? statusquery : [0,1,2,3];
        List<Post> Post_list;
        // Post_list = _Dbcontext.Posts.Where(a => a.Tags.All(b => search_query.Contains(b.Name.ToLower()))).Include(x => x.Tags).Include(y => y.Creator).ToList(); // strict search 
        var Post_list_query = _Dbcontext.Posts.AsQueryable();
        Post_list_query = (textquery_list.Length != 0) ? Post_list_query.Where(post => textquery_list.Any(text_query => post.Title.ToLower().Contains(text_query.Replace("_"," ")) || post.Description.ToLower().Contains(text_query.Replace("_"," ")))) : Post_list_query;
        Post_list_query = (tagquery_list.Length != 0) ? Post_list_query.Where(post => tagquery_list.All(tag_query => post.Tags.Any(tag => tag.Name.ToLower() == tag_query.ToLower()))) : Post_list_query;
        Post_list_query = Post_list_query.Where(post => typequery_list.Contains(post.AcceptType));
        Post_list_query = Post_list_query.Where(post => statusquery_list.Any(status => (int)post.Status == status));
        if (ascending == true)
        {
            switch (orderby)
            {
                case "CreateDate":
                    Post_list_query = Post_list_query.OrderBy(post => post.CreateDate);
                    break;
                case "CloseDate":
                    Post_list_query = Post_list_query.OrderBy(post => post.CloseDate);
                    break;
                case "AmountAccept":
                    Post_list_query = Post_list_query.OrderBy(post => post.AmountAccept);
                    break;
                case "EnrolledCount":
                    Post_list_query = Post_list_query.OrderBy(post => post.EnrolledCount);
                    break;
                default:
                    Post_list_query = Post_list_query.OrderBy(post => post.Id);
                    break;
            }
        }
        else
        {
            switch (orderby)
            {
                case "CreateDate":
                    Post_list_query = Post_list_query.OrderByDescending(post => post.CreateDate);
                    break;
                case "CloseDate":
                    Post_list_query = Post_list_query.OrderByDescending(post => post.CloseDate);
                    break;
                case "AmountAccept":
                    Post_list_query = Post_list_query.OrderByDescending(post => post.AmountAccept);
                    break;
                case "EnrolledCount":
                    Post_list_query = Post_list_query.OrderByDescending(post => post.EnrolledCount);
                    break;
                default:
                    Post_list_query = Post_list_query.OrderByDescending(post => post.Id);
                    break;
            }
        }
        Post_list = Post_list_query.Skip((page - 1) * page_size).Take(page_size).Include(x => x.Tags).Include(y => y.Creator).ToList();
        return Json(Post_list);
    }

    public IActionResult Post(int id)
    {
        var Id = HttpContext.Session.GetInt32("ID");
        if (Id != null)
        {
            var tags = _Dbcontext.Tags.ToList();
            var post = _Dbcontext.Posts.Include(a => a.Creator).Include(b => b.Tags).FirstOrDefault(x => x.Id == id);
            bool is_enrolled = _Dbcontext.Enrollments.FirstOrDefault(a => a.AccountId == Id && a.PostId == id) != null ? true : false;
            bool is_creator = _Dbcontext.Posts.FirstOrDefault(a => a.CreatorId == Id && a.Id == id) != null ? true : false;
            var enrollments = _Dbcontext.Enrollments.Where(a => a.PostId == id).Include(e => e.Account).ToList();
            return View((tags, post, is_enrolled, is_creator, enrollments));
        }
        else 
        {
            TempData["snackbar-message"] = "Your session id has been expired! Login again to continue.";
            TempData["snackbar-type"] = "error";
            return RedirectToAction("Login","Account");
        }
    }

    public IActionResult Result(int id)
    {
        var accountid = HttpContext.Session.GetInt32("ID");
        var post = _Dbcontext.Posts.Include(x => x.Enrollments)!.ThenInclude(y => y.Account).FirstOrDefault(p => p.Id == id);
        if (accountid != null)
        {
            if (post != null && post.Status == PostStatus.Concluded && accountid == post.CreatorId)
            {
                List<Enrollment> enrollment_result;
                bool selectable;
                switch (post.AcceptType)
                {
                    case "OnFull":
                        enrollment_result = post.Enrollments!.OrderBy(x => x.EnrolledAt).Take(post.AmountAccept).ToList();
                        selectable = false;
                        break;
                    case "OnSelect":
                        enrollment_result = post.Enrollments!.OrderBy(x => x.EnrolledAt).ToList();
                        selectable = true;
                        break;
                    case "OnRandom":
                        enrollment_result = post.Enrollments!.OrderBy(x => Random.Shared.Next()).Take(post.AmountAccept).ToList();
                        selectable = false;
                        break;
                    default:
                        enrollment_result = new List<Enrollment>();
                        selectable = false;
                        break;
                }
                return View((enrollment_result, selectable, post.Id, post.Title, post.AmountAccept));
            }
            else
            {
                return Unauthorized();
            }
        }
        else
        {
            TempData["snackbar-message"] = "Your session id has been expired! Login again to continue.";
            TempData["snackbar-type"] = "error";
            return RedirectToAction("Login","Account");
        }
    }

    public IActionResult ConcludeEarly(int id)
    {
        var accountid = HttpContext.Session.GetInt32("ID");
        var post = _Dbcontext.Posts.FirstOrDefault(x => x.Id == id);
        if (accountid != null)
        {
            if (post != null && post.Status == PostStatus.Open && post.CreatorId == accountid)
            {
                post.Status = PostStatus.Concluded;
                var announcement = new Announcement() 
                {
                    PostId = post.Id,
                    Type = AnnouncementType.CreatorUpdate,
                    Message = $"[EARLY] Your post '{post.Title}' has been concluded and is now ready to announce the result!"
                };
                _Dbcontext.Announcements.Add(announcement);
                _Dbcontext.SaveChanges();
                var announcement_recipient = new AnnouncementRecipient()
                {
                    AnnouncementId = announcement.Id,
                    AccountId = post.CreatorId
                };
                _Dbcontext.AnnouncementRecipients.Add(announcement_recipient);
                _Dbcontext.SaveChanges();
                TempData["snackbar-message"] = "Concluded successfully";
                TempData["snackbar-type"] = "success";
                return RedirectToAction("Result","Post", new { id = post.Id });
            }
            else
            {
                return Unauthorized();
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
    public IActionResult CreatePost(Post post, List<int> Tags)
    {
        var Id = HttpContext.Session.GetInt32("ID");
        if (Id != null)
        {
            post.CreateDate = DateTime.Now;
            post.CreatorId = (int)Id;
            if (ModelState.IsValid && Tags.Count > 0 && post.CloseDate > DateTime.Now)
            {
                var tag_list = _Dbcontext.Tags.Where(x => Tags.Contains(x.Id)).ToList();
                foreach (var tag in tag_list)
                {
                    post.Tags.Add(tag);
                }
                _Dbcontext.Posts.Add(post);
                _Dbcontext.SaveChanges();
                _TagStatisticService.UpdateCache();
                TempData["snackbar-message"] = "Creating post successfully";
                TempData["snackbar-type"] = "success";
                return RedirectToAction("Index","Home");
            }
            else 
            {
                var tag_list = _Dbcontext.Tags.Where(x => Tags.Contains(x.Id)).ToList();
                post.Tags = tag_list;
                TempData["Post"] = JsonSerializer.Serialize(post);
                if (ModelState.ErrorCount > 0)
                {
                    var error_message = ModelState.SelectMany(x => x.Value!.Errors).Select(y => y.ErrorMessage).ToList();
                    TempData["snackbar-message"] = error_message[0];
                    TempData["snackbar-type"] = "error";
                }
                else if (!(Tags.Count > 0))
                {
                    TempData["snackbar-message"] = "At least 1 tag is required!";
                    TempData["snackbar-type"] = "error";
                }
                else if (!(post.CloseDate > DateTime.Now)) 
                {
                    TempData["snackbar-message"] = "Close date must be at least 1 minute from now!";
                    TempData["snackbar-type"] = "error";
                }
                return RedirectToAction("Index","Home");
            }
        }
        else
        {
            var tag_list = _Dbcontext.Tags.Where(x => Tags.Contains(x.Id)).ToList();
            post.Tags = tag_list;
            TempData["Post"] = JsonSerializer.Serialize(post);
            TempData["snackbar-message"] = "Your session id has been expired! Login again to continue.";
            TempData["snackbar-type"] = "error";
            return RedirectToAction("Login","Account");
        }
    }

    [HttpPost]
    public IActionResult Post(Post post, List<int> Tags) 
    {
        var Id = HttpContext.Session.GetInt32("ID");
        if (Id != null)
        {
            if (ModelState.IsValid && Tags.Count > 0 && post.CloseDate > DateTime.Now)
            {
                var DBpost = _Dbcontext.Posts.Include(a => a.Tags).FirstOrDefault(x => x.Id == post.Id);
                if (DBpost != null && DBpost.CreatorId == Id && DBpost.Status != PostStatus.Concluded && DBpost.Status != PostStatus.Selected && DBpost.Status != PostStatus.Archived)
                {
                    var tag_list = _Dbcontext.Tags.Where(x => Tags.Contains(x.Id)).ToList();
                    DBpost.Tags.Clear();
                    foreach (var tag in tag_list)
                    {
                        DBpost.Tags.Add(tag);
                    }
                    DBpost.Title = post.Title;
                    DBpost.Description = post.Description;
                    DBpost.Picture = post.Picture;
                    DBpost.AmountAccept = post.AmountAccept;
                    DBpost.AcceptType = post.AcceptType;
                    DBpost.CloseDate = post.CloseDate;
                    _Dbcontext.SaveChanges();
                    _TagStatisticService.UpdateCache();
                    TempData["snackbar-message"] = "Post edited successfully";
                    TempData["snackbar-type"] = "success";
                    return RedirectToAction("Post","Post", new { id = DBpost.Id });
                }
                else
                {
                    return Unauthorized();
                }
            }
            else 
            {
                if (ModelState.ErrorCount > 0)
                {
                    var error_message = ModelState.SelectMany(x => x.Value!.Errors).Select(y => y.ErrorMessage).ToList();
                    TempData["snackbar-message"] = error_message[0];
                    TempData["snackbar-type"] = "error";
                }
                else if (!(Tags.Count > 0))
                {
                    TempData["snackbar-message"] = "At least 1 tag is required!";
                    TempData["snackbar-type"] = "error";
                }
                else if (!(post.CloseDate > DateTime.Now)) 
                {
                    TempData["snackbar-message"] = "Close date must be at least 1 minute from now!";
                    TempData["snackbar-type"] = "error";
                }
                return RedirectToAction("Post","Post", new { id = post.Id });
            }
        }
        else 
        {
            TempData["snackbar-message"] = "Your session id has been expired! Login again to continue.";
            TempData["snackbar-type"] = "error";
            return RedirectToAction("Login","Account");
        }
    }

    public JsonResult Enroll(int PostId) 
    {
        var AccountId = HttpContext.Session.GetInt32("ID");
        if (AccountId != null)
        {
            if (!_Dbcontext.Enrollments.Any(e => e.AccountId == AccountId && e.PostId == PostId))
            {
                var post = _Dbcontext.Posts.FirstOrDefault(x => x.Id == PostId);
                if (post != null)
                {
                    if (post.CreatorId == AccountId || post.Status != PostStatus.Open) 
                    {
                        return Json(new { enroll_successful = false });
                    }
                    post.EnrolledCount++;
                }
                Enrollment enrollment = new Enrollment() {AccountId = (int)AccountId, PostId = PostId, EnrolledAt = DateTime.Now};
                _Dbcontext.Enrollments.Add(enrollment);
                _Dbcontext.SaveChanges();
                return Json(new { enroll_successful = true, enroll_count = post?.EnrolledCount });
            }
            else
            {
                return Json(new { enroll_successful = false });
            }
        }
        else
        {
            TempData["snackbar-message"] = "Your session id has been expired! Login again to continue.";
            TempData["snackbar-type"] = "error";
            return Json(new { enroll_successful = false });
        }
    }

    public JsonResult Unroll(int PostId)
    {
        var AccountId = HttpContext.Session.GetInt32("ID");
        if (AccountId != null)
        {
            var enrollment = _Dbcontext.Enrollments.FirstOrDefault(x => x.AccountId == AccountId && x.PostId == PostId);
            if (enrollment != null)
            {
                var post = _Dbcontext.Posts.FirstOrDefault(x => x.Id == PostId);
                if (post == null || AccountId == post.CreatorId || post.Status != PostStatus.Open)
                {
                    return Json(new { unroll_successful = false });
                }
                post.EnrolledCount--;
                _Dbcontext.Remove(enrollment);
                _Dbcontext.SaveChanges();
                return Json(new { unroll_successful = true, enroll_count = post?.EnrolledCount });
            }
            else
            {
                return Json(new { unroll_successful = false });
            }
        }
        else
        {
            TempData["snackbar-message"] = "Your session id has been expired! Login again to continue.";
            TempData["snackbar-type"] = "error";
            return Json(new { unroll_successful = false });
        }
    }

    [HttpPost]
    public IActionResult Result(int id, List<int> Selection)
    {
        var accountid = HttpContext.Session.GetInt32("ID");
        if (accountid != null)
        {
            var post = _Dbcontext.Posts.FirstOrDefault(x => x.Id == id);
            if (post != null && post.Status == PostStatus.Concluded && accountid == post.CreatorId)
            {
                post.Status = PostStatus.Selected;
                var selected_account = _Dbcontext.Enrollments.Include(x => x.Account).Where(y => Selection.Contains(y.AccountId)).Select(z => z.Account).ToList();
                foreach (var account in selected_account)
                {
                    post.SelectedAccounts!.Add(account!);
                }
                _Dbcontext.SaveChanges();
                TempData["snackbar-message"] = "Saving result successfully";
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
            TempData["snackbar-message"] = "Your session id has been expired! Login again to continue.";
            TempData["snackbar-type"] = "error";
            return RedirectToAction("Login","Account");
        }
    }

    public IActionResult DeletePost(int id)
    {
        var accountid = HttpContext.Session.GetInt32("ID");
        if (accountid != null)
        {
            var post = _Dbcontext.Posts.FirstOrDefault(x => x.Id == id);
            if (post != null && post.CreatorId == accountid && post.Status != PostStatus.Archived)
            {
                _Dbcontext.Posts.Remove(post);
                _Dbcontext.SaveChanges();
                TempData["snackbar-message"] = "Delete post successfully";
                TempData["snackbar-type"] = "success";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return Unauthorized();
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
