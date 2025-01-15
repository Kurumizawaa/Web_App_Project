using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
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
        return View();
    }

    public IActionResult Login(string username)
    {
        bool isValidUser = ValidateUser(username);
        if (isValidUser)
        {
            return View();
        }
        else
        {
            return View();
        }
    }

    private bool ValidateUser(string username)
    {
        var user = _Dbcontext.Accounts.FirstOrDefault(a => a.Username == username);
        return user != null;
    }

    public IActionResult CreateAccount(Account account)
    {
        if (ModelState.IsValid)
        {
            return RedirectToAction("Index","Home");
        }
        else
        {
            return RedirectToAction("Index","Home");
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
