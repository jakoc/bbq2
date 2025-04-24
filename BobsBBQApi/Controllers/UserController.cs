using Microsoft.AspNetCore.Mvc;

namespace BobsBBQApi.Controllers;

public class UserController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}