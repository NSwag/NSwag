using System.Collections.Generic;
using System.Threading.Tasks;
using NJsonSchema;
using Xunit;

namespace NSwag.Core.Tests.Serialization
{
    public class ExampleSerializationTests
    {
        [Fact]
        public void When_document_has_response_examples_then_it_is_serialized_in_Swagger()
        {
            // Arrange
            var document = CreateDocument();

            // Act
            var json = document.ToJson(SchemaType.Swagger2);

            // Assert
            Assert.Contains(@"""examples"": 1", json); // response examples
            Assert.Contains(@"""example"": 2", json); // parameter example
            Assert.DoesNotContain(@"""ParameterExamples""", json); // parameter examples
        }

        [Fact]
        public void When_document_has_response_examples_then_it_is_not_serialized_in_OpenApi()
        {
            // Arrange
            var document = CreateDocument();

            // Act
            var json = document.ToJson(SchemaType.OpenApi3);

            // Assert
            Assert.DoesNotContain(@"""examples"": 1", json); // response examples
            Assert.Contains(@"""example"": 2", json); // parameter example
            Assert.Contains(@"""ParameterExamples""", json); // parameter examples
        }

        private static OpenApiDocument CreateDocument()
        {
            var document = new OpenApiDocument();
            document.Paths["foo"] = new OpenApiPathItem
            {
                {
                    OpenApiOperationMethod.Get,
                    new OpenApiOperation
                    {
                        Parameters =
                        {
                            new OpenApiParameter
                            {
                                Kind = OpenApiParameterKind.Query,
                                Example = 2,
                                Examples = new Dictionary<string, OpenApiExample>
                                {
                                    {
                                        "ParameterExamples",
                                        new OpenApiExample
                                        {
                                            Description = "Bar"
                                        }
                                    }
                                }
                            }
                        },
                        Responses =
                        {
                            {
                                "200",
                                new OpenApiResponse
                                {
                                    Examples = 1
                                }
                            }
                        }
                    }
                }
            };

            return document;
        }
    }
}