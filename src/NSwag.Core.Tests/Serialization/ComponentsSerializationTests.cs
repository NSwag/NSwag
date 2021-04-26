using System.Threading.Tasks;
using NJsonSchema;
using Xunit;

namespace NSwag.Core.Tests.Serialization
{
    public class ComponentsSerializationTests
    {
        [Fact]
        public async Task When_schema_is_added_to_definitions_then_it_is_serialized_correctly_in_Swagger()
        {
            // Arrange
            var document = CreateDocument();

            // Act
            var json = document.ToJson(SchemaType.Swagger2);
            document = await OpenApiDocument.FromJsonAsync(json);

            // Assert
            Assert.Contains(@"""swagger""", json);
            Assert.DoesNotContain(@"""openapi""", json);

            Assert.Contains("definitions", json);
            Assert.DoesNotContain("components", json);

            Assert.True(document.Definitions.ContainsKey("Foo"));
        }

        [Fact]
        public async Task When_schema_is_added_to_definitions_then_it_is_serialized_correctly_in_OpenApi()
        {
            // Arrange
            var document = CreateDocument();

            // Act
            var json = document.ToJson(SchemaType.OpenApi3);
            document = await OpenApiDocument.FromJsonAsync(json);

            // Assert
            Assert.DoesNotContain(@"""swagger""", json);
            Assert.Contains(@"""openapi""", json);

            Assert.Contains("components", json);
            Assert.Contains("schemas", json);
            Assert.DoesNotContain("#/definitions/Foo", json);
            Assert.DoesNotContain("definitions", json);

            Assert.True(document.Definitions.ContainsKey("Foo"));
        }

        [Fact]
        public async Task When_openapi3_has_nullable_schema_then_it_is_read_correctly()
        {
            var json = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""title"": ""Swagger Petstore"",
    ""license"": {
      ""name"": ""MIT""
    },
    ""version"": ""1.0.0""
  },
  ""servers"": [
    {
      ""url"": ""http://petstore.swagger.io/v1""
    }
  ],
  ""paths"": { },
  ""components"": {
    ""schemas"": {
      ""PurchaseReadDto2"": {
        ""type"": ""object"",
        ""nullable"": true,
        ""additionalProperties"": false,
        ""properties"": {
          ""participantsParts"": {
            ""type"": ""object"",
            ""nullable"": true,
            ""additionalProperties"": {
              ""type"": ""number"",
              ""format"": ""decimal"",
              ""nullable"": true
            }
          }
        }
      }
    }
  }
}";
            // Act
            var document = await OpenApiDocument.FromJsonAsync(json);

            // Assert
            Assert.True(document.Components.Schemas["PurchaseReadDto2"].IsNullableRaw);
            Assert.True(document.Components.Schemas["PurchaseReadDto2"].Properties["participantsParts"].IsNullableRaw);
            Assert.True(document.Components.Schemas["PurchaseReadDto2"].Properties["participantsParts"].AdditionalPropertiesSchema.IsNullableRaw);
        }

        private static OpenApiDocument CreateDocument()
        {
            var schema = new JsonSchema
            {
                Type = JsonObjectType.String
            };

            var document = new OpenApiDocument();
            document.Definitions["Foo"] = schema;
            document.Definitions["Bar"] = new JsonSchema
            {
                Type = JsonObjectType.Object,
                Properties =
                {
                    { "Foo", new JsonSchemaProperty { Reference = schema } }
                }
            };

            return document;
        }
    }
}