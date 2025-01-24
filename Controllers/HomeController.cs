using System.Diagnostics;
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
        return View();
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
