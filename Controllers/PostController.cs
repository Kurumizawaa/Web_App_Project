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
    
    public JsonResult GetPost(int page, string query)
    {
        var page_size = 4;
        var tag_list = _Dbcontext.Tags.ToList();
        // if (query != null){
        //     var search_query = query.Split();
        //     var Filtered_Post_list = new List<Post>();
        //     var Post_list = _Dbcontext.Posts.Include(x => x.Tags);
        // }
        var Post_list = _Dbcontext.Posts.Skip((page - 1) * page_size).Take(page_size).Include(x => x.Creator).Include(a => a.Tags).ToList();
        return Json(Post_list);
    }

    public IActionResult Post(int id)
    {
        var tags = _Dbcontext.Tags.ToList();
        var post = _Dbcontext.Posts.Include(a => a.Creator).Include(b => b.Tags).FirstOrDefault(x => x.Id == id);
        return View((tags, post));
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
                if (DBpost != null)
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
                    if (post.CreatorId == AccountId) 
                    {
                        return Json(new { enroll_successful = false });
                    }
                    post.EnrolledCount++;
                }
                Enrollment enrollment = new Enrollment() {AccountId = (int)AccountId, PostId = PostId, EnrolledAt = DateTime.Now};
                _Dbcontext.Enrollments.Add(enrollment);
                _Dbcontext.SaveChanges();
                return Json(new { enroll_successful = true });
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
                _Dbcontext.Remove(enrollment);
                var post = _Dbcontext.Posts.FirstOrDefault(x => x.Id == PostId);
                if (post != null)
                {
                    post.EnrolledCount--;
                }
                _Dbcontext.SaveChanges();
                return Json(new { unroll_successful = true });
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
