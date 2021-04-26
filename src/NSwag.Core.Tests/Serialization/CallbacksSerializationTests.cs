using NJsonSchema;
using Xunit;

namespace NSwag.Core.Tests.Serialization
{
    public class CallbacksSerializationTests
    {
        [Fact]
        public void When_callbacks_are_defined_then_they_are_serialized_in_OpenApi()
        {
            // Arrange
            var document = new OpenApiDocument
            {
                Paths =
                {
                    {
                        "/baz",
                        new OpenApiPathItem
                        {
                            {
                                OpenApiOperationMethod.Get,
                                new OpenApiOperation
                                {
                                    Callbacks =
                                    {
                                        {
                                            "onData",
                                            new OpenApiCallback
                                            {
                                                {
                                                    "foo",
                                                    new OpenApiPathItem
                                                    {
                                                        {
                                                            OpenApiOperationMethod.Post,
                                                            new OpenApiOperation
                                                            {
                                                                Description = "bar"
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Act
            var json = document.ToJson(SchemaType.OpenApi3);

            // Assert
            Assert.Contains(@"""bar""", json);
        }
    }
}