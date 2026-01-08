using Microsoft.AspNetCore.Mvc;

namespace University.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
    public IActionResult AccessDenied() => View();
    public IActionResult Error() => View();
}
