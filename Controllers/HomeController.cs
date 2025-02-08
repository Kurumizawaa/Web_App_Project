using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using web_app_project.Data;
using web_app_project.Models;

namespace web_app_project.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _Dbcontext;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbcontext)
    {
        _logger = logger;
        _Dbcontext = dbcontext;
    }

    public IActionResult Index()
    {
        var Tag_List = _Dbcontext.Tags.ToList();
        var Post_form = new Post();
        var Id = HttpContext.Session.GetInt32("ID");
        if (TempData["Post"] != null)
        {
            Post_form = JsonSerializer.Deserialize<Post>(TempData["Post"]!.ToString()!);
        }
        if (Id == null)
        {
            Id = 0;
        }
        return View((Tag_List, Post_form, Id));
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult CreateTag()
    {
        return RedirectToAction("Index","Home");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
