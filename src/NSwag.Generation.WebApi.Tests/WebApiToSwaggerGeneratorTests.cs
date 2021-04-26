using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSwag.Demo.Web.Controllers;

namespace NSwag.Generation.WebApi.Tests
{
    [TestClass]
    public class WebApiToSwaggerGeneratorTests
    {
        [TestMethod]
        public async Task When_generating_swagger_from_controller_then_all_required_operations_are_available()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync<PersonsController>();
            var swaggerSpecification = document.ToJson();

            // Assert
            Assert.AreEqual(7, document.Operations.Count());
        }

        [TestMethod]
        public async Task When_there_is_a_ResultType_attribute_on_an_action_method_then_the_response_is_taken_from_the_given_type()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings
            {
                DefaultUrlTemplate = "api/{controller}/{action}/{id?}"
            });

            // Act
            var document = await generator.GenerateForControllerAsync<PersonsDefaultRouteController>();
            var operation = document.Operations.Single(o => o.Path == "/api/PersonsDefaultRoute/Get/{id}");
            var json = document.ToJson();

            // Assert
            Assert.AreEqual(2, operation.Operation.ActualResponses.Count);
            Assert.AreEqual(10, document.Operations.Count());
            Assert.IsTrue(document.Definitions.Any(d => d.Key == "Person"));
        }
    }
}
