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
        var Tag_List = new List<string> {"Travel", "Workout", "Eating", "Party"};
        var Post_form = new Post();
        if (TempData["Post"] != null)
        {
            Post_form = JsonSerializer.Deserialize<Post>(TempData["Post"]!.ToString()!);
        }
        return View((Tag_List, Post_form));
    }

    public IActionResult About()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
