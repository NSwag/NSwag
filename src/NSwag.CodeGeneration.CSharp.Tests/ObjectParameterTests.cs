using System.Threading.Tasks;
using Xunit;

namespace NSwag.CodeGeneration.CSharp.Tests
{
    public class ObjectParameterTests
    {
        [Fact]
        public async Task when_content_is_formdata_with_property_object_then_content_should_be_json_csharp()
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
            var settings = new CSharpClientGeneratorSettings { ClassName = "MyClass" };
            var generator = new CSharpClientGenerator(document, settings);
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("var content_ = new System.Net.Http.MultipartFormDataContent(boundary_);", code);
            Assert.Contains("content_.Add(new System.Net.Http.StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(propertyDto, _settings.Value)), \"propertyDto\");", code);
        }
    }
}
