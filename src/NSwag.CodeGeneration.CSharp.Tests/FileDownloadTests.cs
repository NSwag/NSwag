using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Generation.WebApi;
using Xunit;

namespace NSwag.CodeGeneration.CSharp.Tests
{
    public class FileDownloadTests
    {
        public class FileDownloadController : Controller
        {
            [Route("DownloadFile")]
            public HttpResponseMessage DownloadFile()
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public async Task When_response_is_file_and_stream_is_not_used_then_byte_array_is_returned()
        {
            // Arrange
            var swaggerGenerator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await swaggerGenerator.GenerateForControllerAsync<FileDownloadController>();

            // Act
            var codeGen = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings
            {
                GenerateClientInterfaces = true
            });
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("System.Threading.Tasks.Task<FileResponse> DownloadFileAsync();", code);
            Assert.Contains("ReadAsStreamAsync()", code);
        }

        [Fact]
        public async Task When_openapi3_contains_octet_stream_response_then_FileResponse_is_generated()
        {
            // Arrange
            var json = @"{
  ""openapi"": ""3.0.1"",
  ""paths"": {
    ""/instances/{id}/frames/{index}/raw"": {
      ""get"": {
        ""description"": ""sample"",
        ""operationId"": ""raw"",
        ""parameters"": [],
        ""responses"": {
          ""200"": {
            ""description"": ""raw file in binary data"",
            ""content"": {
              ""application/octet-stream"": {
                ""schema"": {
                  ""type"": ""string"",
                  ""format"": ""binary""
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
            Assert.Contains("public async System.Threading.Tasks.Task<FileResponse> RawAsync(", code);
            Assert.Contains("var fileResponse_ = new FileResponse(", code);
        }
    }
}
