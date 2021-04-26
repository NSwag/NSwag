using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSwag.Generation.WebApi;

namespace NSwag.Generation.WebApi.Tests.Attributes
{
    [TestClass]
    public class ApiExplorerSettingsAttributeTests
    {
        public class TestController : ApiController
        {
            [ApiExplorerSettings(IgnoreApi = false)]
            public object Foo()
            {
                return null; 
            }

            [ApiExplorerSettings(IgnoreApi = true)]
            public object Bar()
            {
                return null; 
            }
        }

        [TestMethod]
        public async Task When_action_has_ApiExplorerSettingsAttribute_with_IgnoreApi_then_it_is_ignored()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings { IsAspNetCore = true });

            // Act
            var document = await generator.GenerateForControllerAsync<TestController>();
            var json = document.ToJson();

            // Assert
            Assert.AreEqual(1, document.Operations.Count());
            Assert.AreEqual("Test_Foo", document.Operations.Single().Operation.OperationId);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public class ApiExplorerIgnoredController : ApiController
        {
        }

        [TestMethod]
        public void When_controller_has_ApiExplorerSettingsAttribute_with_IgnoreApi_then_it_is_ignored()
        {
            // Arrange

            // Act
            var controllers = WebApiOpenApiDocumentGenerator.GetControllerClasses(typeof(ApiExplorerIgnoredController).Assembly);

            // Assert
            Assert.IsFalse(controllers.Any(c => c.Name == "ApiExplorerIgnoredController"));
        }
    }
}