using Microsoft.AspNetCore.Mvc;

namespace BobsBBQApi.Controllers;

public class ReservationController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}