using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
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
            var announcement_list = _Dbcontext.AnnouncementRecipients.Where(x => x.AccountId == Id).ToList();
            return Json(announcement_list);
        }
        else
        {
            return Json(null);
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
