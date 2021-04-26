using System.Threading.Tasks;
using Xunit;

namespace NSwag.CodeGeneration.TypeScript.Tests
{
    public class ObjectParameterTests
    {
        [Fact]
        public async Task when_content_is_formdata_with_property_object_then_content_should_be_json_typescript()
        {
            var json = @"{
              ""x-generator"": ""NSwag v13.5.0.0 (NJsonSchema v10.1.15.0 (Newtonsoft.Json v11.0.0.0))"",
              ""openapi"": ""3.0.0"",
              ""info"": {
                ""title"": ""My Title"",
                ""version"": ""1.0.0""
              },
              ""paths"": {
                ""/api/FileUpload/UploadFile"": {
                  ""post"": {
                    ""tags"": [
                      ""FileUpload""
                    ],
                    ""operationId"": ""FileUpload_UploadFile"",
                    ""requestBody"": {
                      ""content"": {
                        ""multipart/form-data"": {
                          ""schema"": {
                            ""properties"": {
                              ""file"": {
                                ""type"": ""string"",
                                ""format"": ""binary""
                              },
                              ""propertyDto"": {
                                ""type"": ""object"",
                                ""description"": ""Configurable properties""
                              },
                              ""test"": {
                                ""type"": ""string""
                              }
                            }
                          }
                        }
                      }
                    },
                    ""responses"": {
                      ""200"": {
                        ""description"": """",
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
                },
    
              },
              ""components"": {}
            }";

            var document = await OpenApiDocument.FromJsonAsync(json);

            // Act
            var codeGenerator = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings());
            var code = codeGenerator.GenerateFile();

            // Assert
            Assert.Contains("const content_ = new FormData();", code);
            Assert.Contains("content_.append(\"propertyDto\", JSON.stringify(propertyDto))", code);
        }
    }
}
