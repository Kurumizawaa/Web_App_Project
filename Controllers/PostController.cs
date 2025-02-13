using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using web_app_project.Data;
using web_app_project.Models;

namespace web_app_project.Controllers;

public class PostController : Controller
{
    private readonly ApplicationDbContext _Dbcontext;

    public PostController(ApplicationDbContext dbcontext)
    {
        _Dbcontext = dbcontext;
    }
    
    public JsonResult GetPost(int page, string textquery, string tagquery)
    {
        var page_size = 4;
        var tagquery_list = (tagquery != null) ? tagquery.ToLower().Split() : [];
        List<Post> Post_list;
        if (textquery != null || tagquery_list.Length != 0)
        {
            // Post_list = _Dbcontext.Posts.Where(a => a.Tags.All(b => search_query.Contains(b.Name.ToLower()))).Include(x => x.Tags).Include(y => y.Creator).ToList(); // strict search 
            var Post_list_query = _Dbcontext.Posts.AsQueryable();
            Post_list_query = (textquery != null) ? Post_list_query.Where(a => a.Title.ToLower().Contains(textquery.ToLower()) || a.Description.ToLower().Contains(textquery.ToLower())) : Post_list_query;
            Post_list_query = (tagquery_list.Length != 0) ? Post_list_query.Where(post => tagquery_list.All(tag_query => post.Tags.Any(tag => tag.Name.ToLower() == tag_query.ToLower()))) : Post_list_query;
            Post_list = Post_list_query.Skip((page - 1) * page_size).Take(page_size).Include(x => x.Tags).Include(y => y.Creator).ToList();
        }
        else {
            Post_list = _Dbcontext.Posts.Skip((page - 1) * page_size).Take(page_size).Include(x => x.Tags).Include(y => y.Creator).ToList();
        }
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
            return View((tags, post, is_enrolled, is_creator));
        }
        else 
        {
            TempData["Info"] = "Your session id has been expired! Login again to continue.";
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

    public IActionResult ConcludeEarly(int id)
    {
        var accountid = HttpContext.Session.GetInt32("ID");
        var post = _Dbcontext.Posts.FirstOrDefault(x => x.Id == id);
        if (accountid != null)
        {
            if (post != null && post.Status == PostStatus.Open && post.CreatorId == accountid)
            {
                post.Status = PostStatus.Concluded;
                _Dbcontext.SaveChanges();
                return RedirectToAction("Result","Post", new { id = post.Id });
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
    public IActionResult CreatePost(Post post, List<int> Tags)
    {
        var Id = HttpContext.Session.GetInt32("ID");
        if (Id != null)
        {
            post.CreateDate = DateTime.Now;
            post.CreatorId = (int)Id;
            if (ModelState.IsValid)
            {
                var tag_list = _Dbcontext.Tags.Where(x => Tags.Contains(x.Id)).ToList();
                foreach (var tag in tag_list)
                {
                    post.Tags.Add(tag);
                }
                _Dbcontext.Posts.Add(post);
                _Dbcontext.SaveChanges();
                return RedirectToAction("Index","Home");
            }
            else 
            {
                var errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList();

                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
                TempData["ErrorMessage"] = "At least 1 tag is required!";
                return RedirectToAction("Index","Home");
            }
        }
        else
        {
            var tag_list = _Dbcontext.Tags.Where(x => Tags.Contains(x.Id)).ToList();
            post.Tags = tag_list;
            TempData["Info"] = "Your session id has been expired! Login again to continue.";
            TempData["Post"] = JsonSerializer.Serialize(post);
            return RedirectToAction("Login","Account");
        }
    }

    [HttpPost]
    public IActionResult Post(Post post, List<int> Tags) 
    {
        var Id = HttpContext.Session.GetInt32("ID");
        if (Id != null)
        {
            if (ModelState.IsValid)
            {
                var DBpost = _Dbcontext.Posts.Include(a => a.Tags).FirstOrDefault(x => x.Id == post.Id);
                if (DBpost != null && DBpost.Status != PostStatus.Concluded && DBpost.Status != PostStatus.Archived)
                {
                    if (DBpost.CreatorId != Id) {return RedirectToAction("Account","Account");} //handle this later
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
                    return RedirectToAction("Post","Post", new { id = DBpost.Id });
                }
                else
                {
                    //handle this later
                    return RedirectToAction("Index","Home");
                }
            }
            else 
            {
                var errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList();

                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
                TempData["ErrorMessage"] = "At least 1 tag is required!";
                return RedirectToAction("Index","Home");
            }
        }
        else 
        {
            TempData["Info"] = "Your session id has been expired! Login again to continue.";
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
                //handle this later
                return Json(new { enroll_successful = false });
            }
        }
        else
        {
            TempData["Info"] = "Your session id has been expired! Login again to continue.";
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
                //handle this later
                return Json(new { unroll_successful = false });
            }
        }
        else
        {
            TempData["Info"] = "Your session id has been expired! Login again to continue.";
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
                return RedirectToAction("PostResultAnnouncement","Announcement", new { postid = post.Id });
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
