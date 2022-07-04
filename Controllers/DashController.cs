using Microsoft.AspNetCore.Mvc;

namespace RockFS.Controllers;

public class DashController : Controller
{
    public IActionResult Mounts()
    {
        //
        return View();
    }
}