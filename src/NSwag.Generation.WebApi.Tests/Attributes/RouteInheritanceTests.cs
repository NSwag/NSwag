using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NSwag.Generation.WebApi.Tests.Attributes
{
    // See https://github.com/RicoSuter/NSwag/issues/48

    [TestClass]
    public class RouteInheritanceTests
    {
        [TestMethod]
        public async Task When_route_is_on_inherited_parent_class_and_route_prefix_on_class_then_it_is_used_for_swagger_generation()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync<MyController>();

            // Assert
            Assert.AreEqual("/api/My/Foo", document.Operations.First().Path);
        }

        [RoutePrefix("api/My")]
        public class MyController : BaseController
        {

        }

        public class BaseController : ApiController
        {
            [Route("Foo")]
            public string Bar()
            {
                return "foo";
            }
        }

        [TestMethod]
        public async Task When_route_is_on_inherited_parent_class_then_it_is_used_for_swagger_generation()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync<MyController2>();

            // Assert
            Assert.AreEqual("/Foo", document.Operations.First().Path);
        }

        public class MyController2 : BaseController2
        {

        }

        public class BaseController2 : ApiController
        {
            [Route("Foo")]
            public string Bar()
            {
                return "foo";
            }
        }
    }
}
