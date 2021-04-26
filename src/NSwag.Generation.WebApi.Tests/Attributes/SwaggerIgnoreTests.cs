using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSwag.Annotations;

namespace NSwag.Generation.WebApi.Tests.Attributes
{
    [TestClass]
    public class SwaggerIgnoreTests
    {
        public class MyController : ApiController
        {
            [Route("api/{id}/service")]
            public void Foo([SwaggerIgnore] string id)
            {

            }
        }

        [TestMethod]
        public async Task When_parameter_is_ignored_then_the_route_is_correct()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync<MyController>();

            // Assert
            Assert.AreEqual("/api/service", document.Paths.First().Key);
        }
    }
}
