using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSwag.Annotations;

namespace NSwag.Generation.WebApi.Tests.Attributes
{
    [TestClass]
    public class ExtensionDataTests
    {
        [SwaggerExtensionData("a", "b")]
        [SwaggerExtensionData("x", "y")]
        public class TagsTest1Controller : ApiController
        {

        }

        [TestMethod]
        public async Task When_controller_has_extension_data_attributes_then_they_are_processed()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync<TagsTest1Controller>();

            // Assert
            Assert.AreEqual(2, document.ExtensionData.Count);

            Assert.AreEqual("b", document.ExtensionData["a"]);
            Assert.AreEqual("y", document.ExtensionData["x"]);
        }

        public class TagsTest3Controller : ApiController
        {
            [SwaggerExtensionData("a", "b")]
            [SwaggerExtensionData("x", "y")]
            public void MyAction([SwaggerExtensionData("c", "d")] string foo)
            {

            }
        }

        [TestMethod]
        public async Task When_operation_has_extension_data_attributes_then_they_are_processed()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync<TagsTest3Controller>();

            // Assert
            var extensionData = document.Operations.First().Operation.ExtensionData;
            Assert.AreEqual(2, extensionData.Count);

            Assert.AreEqual("b", document.Operations.First().Operation.ExtensionData["a"]);
            Assert.AreEqual("y", document.Operations.First().Operation.ExtensionData["x"]);
            Assert.AreEqual("foo", document.Operations.First().Operation.Parameters.First().Name);
            Assert.AreEqual("d", document.Operations.First().Operation.Parameters.First().ExtensionData["c"]);
        }
    }
}
