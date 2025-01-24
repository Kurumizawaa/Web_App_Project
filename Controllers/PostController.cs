using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
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
    
    public IActionResult PostSection()
    {
        return PartialView();
    }

    [HttpPost]
    public IActionResult CreatePost(Post post)
    {
        var CreateDate = DateTime.Now; 
        post.CreateDate = CreateDate;
        Console.WriteLine(post.CreaterId);
        if (ModelState.IsValid)
        {
            return RedirectToAction("Index","Home");
        }
        else 
        {
            return RedirectToAction("Account","Account");
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
