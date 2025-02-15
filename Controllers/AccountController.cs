using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_app_project.Data;
using web_app_project.Models;

namespace web_app_project.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _Dbcontext;

    public AccountController(ApplicationDbContext dbcontext)
    {
        _Dbcontext = dbcontext;
    }

    public IActionResult Account()
    {
        var Id = HttpContext.Session.GetInt32("ID");
        if (Id != null)
        {
            ViewBag.Account = _Dbcontext.Accounts.FirstOrDefault(x => x.Id == Id);
            return View();
        }
        else
        {
            TempData["snackbar-message"] = "Your session id has been expired! Login again to continue.";
            TempData["snackbar-type"] = "error";
            return RedirectToAction("Login","Account");
        }
    }

    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Remove("ID");
        TempData["snackbar-message"] = "Logout successfully";
        TempData["snackbar-type"] = "success";
        return RedirectToAction("Login","Account");
    }

    [HttpPost]
    public IActionResult Account(Account account) 
    {
        var Id = HttpContext.Session.GetInt32("ID");
        if (Id != null)
        {
            var DBaccount = _Dbcontext.Accounts.FirstOrDefault(x => x.Id == Id);
            if (DBaccount != null)
            {
                DBaccount.Username = account.Username;
                DBaccount.ProfilePicture = account.ProfilePicture;
                DBaccount.Bio = account.Bio;
                DBaccount.Password = account.Password;
                _Dbcontext.SaveChanges();
                ViewBag.Account = DBaccount;
                TempData["snackbar-message"] = "Account edited successfully";
                TempData["snackbar-type"] = "success";
                return View();
            }
            else 
            {
                TempData["snackbar-message"] = "Your session id has been expired! Login again to continue.";
                TempData["snackbar-type"] = "error";
                return RedirectToAction("Login","Account");
            }
        }
        else
        {
            TempData["snackbar-message"] = "Your session id has been expired! Login again to continue.";
            TempData["snackbar-type"] = "error";
            return RedirectToAction("Login","Account");
        }
    }

    public JsonResult GetAccountPosts(){
        var Id = HttpContext.Session.GetInt32("ID");

        var posts_created = _Dbcontext.Posts  
                                      .Where(x => x.CreatorId == Id)
                                      .ToList();

        var posts_enrolled = _Dbcontext.Enrollments
                                        .Include(e => e.Post)
                                        .ThenInclude(p => p!.SelectedAccounts) // Include SelectedAccounts
                                        .Where(x => x.AccountId == Id)
                                        .Select(x => x.Post)
                                        .ToList();
        
        var post_list = new { createdPosts = posts_created, enrolledPosts = posts_enrolled };
        return Json(post_list);
    }

    [HttpPost]
    public IActionResult Login(Account account)
    {
        var User = GetValidateUser(account.Username, account.Password);
        if (User != null)
        {
            HttpContext.Session.SetInt32("ID", User.Id);
            return RedirectToAction("Account","Account");
        }
        else
        {
            TempData["snackbar-message"] = "User not found";
            TempData["snackbar-type"] = "error";
            return View(account);
        }
    }

    private Account? GetValidateUser(string? username, string? Password)
    {
        var user = _Dbcontext.Accounts.FirstOrDefault(a => a.Username == username && a.Password == Password);
        return user;
    }

    [HttpPost]
    public IActionResult Register(Account account)
    {
        if (ModelState.IsValid)
        {
            if (!_Dbcontext.Accounts.Any(a => a.Username == account.Username))
            {
                _Dbcontext.Accounts.Add(account);
                _Dbcontext.SaveChanges();
                TempData["snackbar-message"] = "Account registerd successfully";
                TempData["snackbar-type"] = "success";
                return RedirectToAction("Account","Account");
            }
            else
            {
                TempData["snackbar-message"] = "This name has already been used! Try something else.";
                TempData["snackbar-type"] = "error";
                return View(account);
            }
        }
        else
        {
            TempData["snackbar-message"] = "Register fail";
            TempData["snackbar-type"] = "error";
            return View(account);
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
