using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using web_app_project.Models;

namespace web_app_project.Controllers;

public class PostController : Controller
{
    public IActionResult PostSection()
    {
        return PartialView();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
