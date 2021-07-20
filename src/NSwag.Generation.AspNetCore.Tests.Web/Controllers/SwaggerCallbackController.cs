using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace NSwag.Generation.AspNetCore.Tests.Web.Controllers
{
    [ApiController]
    [Route("api/swaggerCallback")]
    public class SwaggerCallbackController : Controller
    {

        public class MyPayload
        {
            public int MyProperty { get; set; }
        }


        [OpenApiCallback("url1")]
        [OpenApiCallback("url2", "emptyGet", "get")]
        [OpenApiCallback("url3", "single", type : typeof(string))]
        [OpenApiCallback("url4", "multiple", types: new[] { typeof(string), typeof(MyPayload) })]
        public void MyAction(string foo)
        {

        }
    }
}