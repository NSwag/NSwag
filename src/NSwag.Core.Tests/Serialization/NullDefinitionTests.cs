using System.Threading.Tasks;
using Xunit;

namespace NSwag.Core.Tests.Serialization
{
    public class NullDefinitionTests
    {
        [Fact]
        public async Task When_document_has_null_schema_definition_then_it_is_ignored()
        {
            // Arrange
            var json = @"{ ""definitions"": { ""definitions"": null } }";

            // Act
            var document = await OpenApiDocument.FromJsonAsync(json);

            // Assert
            Assert.True(document.Definitions.Count == 0);
        }
    }
}