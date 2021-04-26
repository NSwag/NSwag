using System.Linq;
using System.Threading.Tasks;
using NSwag.Generation.AspNetCore.Tests.Web.Controllers;
using Xunit;

namespace NSwag.Generation.AspNetCore.Tests
{
    public class ExtensionDataTests : AspNetCoreTestsBase
    {
        [Fact]
        public async Task When_controller_has_extension_data_attributes_then_they_are_processed()
        {
            // Arrange
            var settings = new AspNetCoreOpenApiDocumentGeneratorSettings();

            // Act
            var document = await GenerateDocumentAsync(settings, typeof(SwaggerExtensionDataController));

            // Assert
            Assert.Equal(2, document.ExtensionData.Count);

            Assert.Equal("b", document.ExtensionData["a"]);
            Assert.Equal("y", document.ExtensionData["x"]);
        }

        [Fact]
        public async Task When_operation_has_extension_data_attributes_then_they_are_processed()
        {
            // Arrange
            var settings = new AspNetCoreOpenApiDocumentGeneratorSettings();

            // Act
            var document = await GenerateDocumentAsync(settings, typeof(SwaggerExtensionDataController));

            // Assert
            var extensionData = document.Operations.First().Operation.ExtensionData;
            Assert.Equal(2, extensionData.Count);

            Assert.Equal("b", document.Operations.First().Operation.ExtensionData["a"]);
            Assert.Equal("y", document.Operations.First().Operation.ExtensionData["x"]);
            Assert.Equal("foo", document.Operations.First().Operation.Parameters.First().Name);
            Assert.Equal("d", document.Operations.First().Operation.Parameters.First().ExtensionData["c"]);
        }
    }
}
