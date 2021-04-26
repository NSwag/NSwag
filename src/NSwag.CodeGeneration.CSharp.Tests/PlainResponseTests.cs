using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Generation.WebApi;
using Xunit;

namespace NSwag.CodeGeneration.CSharp.Tests
{
    public class PlainResponseTests
    {
        [Fact]
        public async Task When_openapi3_reponse_contains_plain_text_then_Convert_is_generated()
        {
            // Arrange
            var json = @"{
  ""openapi"": ""3.0.1"",
  ""paths"": {
    ""/instances/text"": {
      ""get"": {
        ""description"": ""sample"",
        ""operationId"": ""plain"",
        ""parameters"": [],
        ""responses"": {
          ""200"": {
            ""description"": ""plain string return"",
            ""content"": {
              ""text/plain"": {
                ""schema"": {
                  ""type"": ""string"",
                }
              }
            }
          }
        }
      }
    }
  }
}";
            var document = await OpenApiDocument.FromJsonAsync(json);

            // Act
            var codeGenerator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings
            {
                GenerateClientInterfaces = true
            });
            var code = codeGenerator.GenerateFile();

            // Assert
            Assert.Contains("public async System.Threading.Tasks.Task<string> PlainAsync(", code);
        }

        [Fact]
        public async Task When_swagger_reponse_contains_plain_text_then_Convert_is_generated()
        {
            // Arrange
            var swagger = @"{
  ""swagger"" : ""2.0"",
  ""paths"" : {
    ""/instances/text"": {
      ""get"": {
        ""description"": ""sample"",
        ""operationId"": ""plain"",
        ""produces"": [ ""text/plain"" ],
        ""responses"": {
          ""200"": {
            ""description"": ""plain string return"",
            ""schema"": {
                ""type"": ""string"",
              }
          }
        }
      }
    }
  }
}";
            var document = await OpenApiDocument.FromJsonAsync(swagger);

            // Act
            var codeGenerator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings
            {
                GenerateClientInterfaces = true
            });
            var code = codeGenerator.GenerateFile();

            // Assert
            Assert.Contains("public async System.Threading.Tasks.Task<string> PlainAsync(", code);
        }
    }
}
