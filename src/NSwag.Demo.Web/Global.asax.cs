using System.Web.Http;
using System.Web.Routing;
using NSwag.AspNet.Owin;

namespace NSwag.Demo.Web
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            RouteTable.Routes.MapOwinPath("swagger", app =>
            {
                app.UseSwaggerUi3(typeof(WebApiApplication).Assembly, s =>
                {
                    s.GeneratorSettings.DefaultUrlTemplate = "api/{controller}/{action}/{id}";
                    s.MiddlewareBasePath = "/swagger";
                });
            });


            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
