using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NSwag.SwaggerGeneration.AspNetCore.Tests
{
    public class NestedParametersTests : AspNetCoreTestsBase
    {
        [Fact]
        public async Task When_using_parameters_in_nested_object_then_parameter_attributes_are_processed_correctly()
        {
            // Arrange
            var settings = new AspNetCoreToSwaggerGeneratorSettings();

            // Act
            var document = await GenerateDocumentAsync(settings);
            var json = document.ToJson();

            // Assert
            var operation = GetControllerOperations(document, "NestedParameters").First();
            Assert.Contains(operation.Operation.Parameters, p => p.Kind == SwaggerParameterKind.Path && p.Name == "SubscriptionId");
            Assert.Contains(operation.Operation.Parameters, p => p.Kind == SwaggerParameterKind.Body && p.Name == "Quantity");
        }
    }
}