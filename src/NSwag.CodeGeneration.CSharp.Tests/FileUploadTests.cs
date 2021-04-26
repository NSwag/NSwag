using System.Threading.Tasks;
using NJsonSchema;
using Xunit;

namespace NSwag.CodeGeneration.CSharp.Tests
{
    public class FileUploadTests
    {
        [Fact]
        public async Task When_openapi3_contains_octet_stream_response_then_FileResponse_is_generated()
        {
            // Arrange
            var json = @"{
  ""x-generator"": ""NSwag v13.0.6.0 (NJsonSchema v10.0.23.0 (Newtonsoft.Json v12.0.0.0))"",
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""title"": ""Apimundo API"",
    ""version"": ""1.0.0""
  },
  ""paths"": {
    ""/api/endpoints/{endpointId}/environments/{environmentId}/documents/openapi/compare"": {
      ""post"": {
        ""tags"": [
          ""Endpoints""
        ],
        ""summary"": ""Compares a new document against an existing document."",
        ""operationId"": ""Endpoints_CompareOpenApiDocument"",
        ""parameters"": [],
        ""requestBody"": {
          ""content"": {
            ""application/json"": {
              ""schema"": {
                ""type"": ""file""
              }
            }
          }
        },
        ""responses"": {
          ""200"": {
            ""description"": ""The comparison result."",
            ""content"": {
              ""application/json"": {
                ""schema"": {
                  ""type"": ""string""
                }
              }
            }
          }
        }
      }
    }
  },
  ""components"": {
    ""schemas"": { }
  }
}";
            var document = await OpenApiDocument.FromJsonAsync(json, null, SchemaType.OpenApi3, null);

            // Act
            var codeGenerator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings
            {
                GenerateClientInterfaces = true
            });
            var code = codeGenerator.GenerateFile();

            // Assert
            Assert.Contains("var content_ = new System.Net.Http.StreamContent(body);", code);
        }
    }
}
