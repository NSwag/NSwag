using NJsonSchema;
using Xunit;

namespace NSwag.Core.Tests.Serialization
{
    public class MediaTypesSerializationTests
    {
        [Fact]
        public void When_response_schema_and_example_is_set_then_it_is_serialized_correctly_in_Swagger()
        {
            // Arrange
            var document = CreateDocument(JsonObjectType.String);

            // Act
            var json = document.ToJson(SchemaType.Swagger2);

            // Assert
            Assert.Contains(
@"  ""paths"": {
    ""/foo"": {
      ""get"": {
        ""operationId"": ""foo"",
        ""responses"": {
          ""200"": {
            ""description"": """",
            ""schema"": {
              ""type"": ""string""
            },
            ""examples"": 123
          }
        }
      }
    }
  }
}".Replace("\r", ""), json.Replace("\r", ""));
        }

        [Fact]
        public void When_response_schema_and_example_is_set_then_it_is_serialized_correctly_in_OpenApi()
        {
            // Arrange
            var document = CreateDocument(JsonObjectType.String);

            // Act
            var json = document.ToJson(SchemaType.OpenApi3);

            // Assert
            Assert.Equal(
@"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""title"": ""Swagger specification"",
    ""version"": ""1.0.0""
  },
  ""paths"": {
    ""/foo"": {
      ""get"": {
        ""operationId"": ""foo"",
        ""responses"": {
          ""200"": {
            ""description"": """",
            ""content"": {
              ""application/json"": {
                ""schema"": {
                  ""type"": ""string""
                },
                ""example"": 123
              }
            }
          }
        }
      }
    }
  },
  ""components"": {}
}".Replace("\r", ""), json.Replace("\r", ""));
        }

        [Fact]
        public void When_response_schema_and_example_is_set_as_file_then_it_is_serialized_correctly_in_Swagger()
        {
            // Arrange
            var document = CreateDocument(JsonObjectType.File);

            // Act
            var json = document.ToJson(SchemaType.Swagger2);

            // Assert
            Assert.Contains(
                @"  ""paths"": {
    ""/foo"": {
      ""get"": {
        ""operationId"": ""foo"",
        ""responses"": {
          ""200"": {
            ""description"": """",
            ""schema"": {
              ""type"": ""file""
            },
            ""examples"": 123
          }
        }
      }
    }
  }
}".Replace("\r", ""), json.Replace("\r", ""));
        }

        [Fact]
        public void When_response_schema_and_example_is_set_as_file_then_it_is_serialized_correctly_in_OpenApi()
        {
            // Arrange
            var document = CreateDocument(JsonObjectType.File);

            // Act
            var json = document.ToJson(SchemaType.OpenApi3);

            // Assert
            Assert.Equal(
                @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""title"": ""Swagger specification"",
    ""version"": ""1.0.0""
  },
  ""paths"": {
    ""/foo"": {
      ""get"": {
        ""operationId"": ""foo"",
        ""responses"": {
          ""200"": {
            ""description"": """",
            ""content"": {
              ""application/octet-stream"": {
                ""schema"": {
                  ""type"": ""file""
                },
                ""example"": 123
              }
            }
          }
        }
      }
    }
  },
  ""components"": {}
}".Replace("\r", ""), json.Replace("\r", ""));
        }


        private static OpenApiDocument CreateDocument(JsonObjectType type)
        {
            var document = new OpenApiDocument
            {
                Paths =
                {
                    {
                        "/foo",
                        new OpenApiPathItem
                        {
                            {
                                OpenApiOperationMethod.Get, 
                                new OpenApiOperation
                                {
                                    Responses =
                                    {
                                        {
                                            "200", 
                                            new OpenApiResponse
                                            {
                                                Examples = 123,
                                                Schema = new JsonSchema
                                                {
                                                    Type = type
                                                },
                                            }
                                        }
                                    }
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