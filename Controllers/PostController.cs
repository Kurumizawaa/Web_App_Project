using System.Diagnostics;
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
    
    public JsonResult GetPost()
    {
        var Post_list = _Dbcontext.Posts.Include(x => x.Creator).ToList();
        return Json(Post_list);
    }

    [HttpPost]
    public IActionResult CreatePost(Post post)
    {
        var Id = HttpContext.Session.GetInt32("ID");
        if (Id != null)
        {
            post.CreateDate = DateTime.Now;
            post.CreaterId = (int)Id;
            if (ModelState.IsValid)
            {
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
