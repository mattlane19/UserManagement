using Microsoft.AspNetCore.Authorization;

namespace UserManagement.WebMS.Controllers;

[Authorize]
public class HomeController : Controller
{
    [HttpGet]
    public ViewResult Index() => View();
}
