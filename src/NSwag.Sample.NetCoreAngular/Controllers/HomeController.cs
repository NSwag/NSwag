using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace NSwag_Sample_NetCoreAngular.Controllers
{
    [OpenApiIgnore]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
