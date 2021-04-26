using System.Linq;
using Xunit;

namespace NSwag.Core.Tests
{
    public class OperationIdTests
    {
        [Fact]
        public void When_generating_operation_ids_then_all_are_set()
        {
            // Arrange
            var document = new OpenApiDocument();
            document.Paths["path"] = new OpenApiPathItem
            {
                {
                    OpenApiOperationMethod.Get,
                    new OpenApiOperation { }
                },
                {
                    OpenApiOperationMethod.Post,
                    new OpenApiOperation { }
                }
            };

            // Act
            document.GenerateOperationIds();

            // Assert
            Assert.True(document.Operations.GroupBy(o => o.Operation.OperationId).All(g => g.Count() == 1));
        }
    }
}
