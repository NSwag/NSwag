using System.Web.Mvc;
using NSwag.Annotations;

namespace NSwag.Sample.NetGlobalAsax.Controllers
{
    [OpenApiIgnore]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
    }
}
