using Microsoft.AspNetCore.Mvc;

namespace NSwag.SwaggerGeneration.AspNetCore.Tests.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NestedParametersController : ControllerBase
    {
        [HttpPut]
        [Route("{SubscriptionId}/Quantity")]
        public ActionResult ChangeQuantity(ChangeQuantityRequest request) => Ok();

        public class ChangeQuantityRequest
        {
            [FromRoute]
            public long SubscriptionId { get; set; }

            [FromBody]
            public int Quantity { get; set; }
        }
    }
}