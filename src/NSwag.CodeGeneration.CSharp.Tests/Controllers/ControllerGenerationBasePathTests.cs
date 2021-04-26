using System.Threading.Tasks;
using Xunit;

namespace NSwag.CodeGeneration.CSharp.Tests
{
    public class ControllerGenerationBasePathTests
    {
        [Fact]
        public async Task When_custom_BasePath_is_not_specified_then_the_BasePath_from_document_is_used_as_Route()
        {
            // Arrange
            var document = await OpenApiDocument.FromJsonAsync(_swagger);

            // Act
            var settings = new CSharpControllerGeneratorSettings();
            var generator = new CSharpControllerGenerator(document, settings);
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("Route(\"virtual_directory/v1\")]", code);
        }

        [Fact]
        public async Task When_custom_BasePath_is_specified_then_that_is_used_as_Route()
        {
            // Arrange
            var document = await OpenApiDocument.FromJsonAsync(_swagger);

            // Act
            var settings = new CSharpControllerGeneratorSettings { BasePath = "v1" };
            var generator = new CSharpControllerGenerator(document, settings);
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("Route(\"v1\")]", code);
        }


        private readonly string _swagger = @"{
          ""swagger"" : ""2.0"",
          ""info"" : {
            ""version"" : ""1.0.2"",
            ""title"" : ""Test API""
          },
          ""host"" : ""localhost:8080"",
          ""basePath"" : ""/virtual_directory/v1"",
          ""tags"" : [ {
            ""name"" : ""api""
          } ],
          ""schemes"" : [ ""http"" ],
          ""paths"" : {
             ""/removeElement"" : {
              ""delete"" : {
                ""tags"" : [ ""api"" ],
                ""summary"" : ""Removes elements"",
                ""description"" : ""Removes elements"",
                ""operationId"" : ""removeElement"",
                ""consumes"" : [ ""application/json"" ],
                ""produces"" : [ ""application/json"" ],
                ""parameters"" : [ {
                  ""name"" : ""X-User"",
                  ""in"" : ""header"",
                  ""description"" : ""User identifier"",
                  ""required"" : true,
                  ""type"" : ""string""
                }, {
                  ""name"" : ""elementId"",
                  ""in"" : ""query"",
                  ""description"" : ""The ids of existing elements that should be removed"",
                  ""required"" : false,
                  ""type"" : ""array"",
                  ""items"" : {
                    ""type"" : ""integer"",
                    ""format"" : ""int64""
                  },
                } ],
                ""responses"" : {
                  ""default"" : {
                    ""description"" : ""successful operation""
                  }
                }
              }
            }
          },
            ""definitions"" : { }
        }
        ";
    }
}