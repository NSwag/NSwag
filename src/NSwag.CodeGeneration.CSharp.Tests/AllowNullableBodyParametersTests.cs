using Microsoft.AspNetCore.Mvc;
using NSwag.CodeGeneration.OperationNameGenerators;
using NSwag.Generation.WebApi;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Xunit;

namespace NSwag.CodeGeneration.CSharp.Tests
{
    public class AllowNullableBodyParametersTests
    {
        [Fact]
        public async Task TestNoGuardForOptionalBodyParameter()
        {
            // Arrange
            var swagger =
@"{
  ""openapi"": ""3.0.1"",
  ""paths"": {
    ""/definitions/{definitionId}/elements"": {
      ""get"": {
        ""operationId"": ""elements_LIST_1"",
        ""requestBody"": {
          ""content"": {
            ""*/*"": {
              ""schema"": {
                ""type"": ""integer"",
                ""format"": ""int64""
              }
            }
          }
        },
        ""responses"": {
          ""200"": {
            ""description"": ""Success""
          }
        }
      }
    }
  }
}";
            var document = await OpenApiDocument.FromJsonAsync(swagger);

            // Act
            var codeGen = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings()
            {
                UseBaseUrl = false,
                GenerateClientInterfaces = true,
                OperationNameGenerator = new SingleClientFromOperationIdOperationNameGenerator()
            });

            var code = codeGen.GenerateFile();

            // Assert
            Assert.DoesNotContain("throw new System.ArgumentNullException(\"body\")", code);
        }

        [Fact]
        public async Task TestNullableBodyWithAllowNullableBodyParameters()
        {
            // Arrange
            var generator = await GenerateCode(true);

            // Act
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("throw new System.ArgumentNullException(\"requiredBody\")", code);
            Assert.DoesNotContain("throw new System.ArgumentNullException(\"notRequiredBody\")", code);
        }

        [Fact]
        public async Task TestNullableBodyWithoutAllowNullableBodyParameters()
        {
            // Arrange
            var generator = await GenerateCode(false);

            // Act
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("throw new System.ArgumentNullException(\"requiredBody\")", code);
            Assert.Contains("throw new System.ArgumentNullException(\"notRequiredBody\")", code);
        }

        private static async Task<CSharpClientGenerator> GenerateCode(bool allowNullableBodyParameters)
        {
            var swaggerGenerator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings
            {
                AllowNullableBodyParameters = allowNullableBodyParameters
            });
            var document = await swaggerGenerator.GenerateForControllerAsync<TestController>();

            var generator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings
            {
                InjectHttpClient = false,
                ConfigurationClass = "MyConfig",
                ClientBaseClass = "MyBaseClass"
            });
            return generator;
        }

        public class TestController : Controller
        {
            [Route("Foo")]
            public string Foo([FromBody][Required] T requiredBody)
            {
                return string.Empty;
            }

            [Route("Bar")]
            public void Bar([FromBody] T notRequiredBody)
            {
            }

            public class T
            {
            }
        }
    }
}