using System.Threading.Tasks;
using Xunit;

namespace NSwag.CodeGeneration.TypeScript.Tests
{
    public class ArrayParameterTests
    {
        [Fact]
        public async Task When_parameter_is_array_then_TypeScript_is_correct()
        {
            // Arrange
            var swagger = @"{
  ""swagger"" : ""2.0"",
  ""info"" : {
    ""version"" : ""1.0.2"",
    ""title"" : ""Test API""
  },
  ""host"" : ""localhost:8080"",
  ""basePath"" : ""/"",
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
            var document = await OpenApiDocument.FromJsonAsync(swagger);

            // Act
            var settings = new TypeScriptClientGeneratorSettings { ClassName = "MyClass" };
            var generator = new TypeScriptClientGenerator(document, settings);
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains(@"elementId.forEach(item => { url_ += ""elementId="" + encodeURIComponent("""" + item) + ""&""; });", code);
        }


        [Fact]
        public async Task when_content_is_formdata_with_property_array_then_content_should_be_added_in_foreach_in_typescript()
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
                              ""arrayOfIds"": {
                                ""uniqueItems"": true,
                                ""type"": ""array"",
                                ""items"": {
                                  ""type"": ""string""
                                },
                                ""description"": ""Hash Set of of strings in the DTO request"",
                                ""nullable"": true
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
            Assert.Contains("arrayOfIds.forEach(item_ => content_.append(\"arrayOfIds\", item_.toString()));", code);
        }
    }
}
