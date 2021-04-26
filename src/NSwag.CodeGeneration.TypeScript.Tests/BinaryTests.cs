using System.Threading.Tasks;
using Xunit;

namespace NSwag.CodeGeneration.TypeScript.Tests
{
    public class BinaryTests
    {
        [Fact]
        public async Task When_body_is_binary_then_blob_is_used_as_parameter_in_TypeScript()
        {
            var yaml = @"openapi: 3.0.0
servers:
  - url: https://www.example.com/
info:
  version: '2.0.0'
  title: 'Test API'   
paths:
  /files:
    post:
      tags:
        - Files
      summary: 'Add File'
      operationId: addFile
      responses:
        '200':
          content:
            application/xml:
              schema:
                $ref: '#/components/schemas/FileToken'
      requestBody:
        content:
          image/png:
            schema:
              type: string
              format: binary
components:
  schemas:
    FileToken:
      type: object
      required:
        - fileId    
      properties:  
        fileId:
          type: string
          format: uuid";

            var document = await OpenApiYamlDocument.FromYamlAsync(yaml);

            // Act
            var codeGenerator = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings());
            var code = codeGenerator.GenerateFile();

            // Assert
            Assert.Contains("addFile(body: Blob | undefined): ", code);
            Assert.Contains("\"Content-Type\": \"image/png\"", code);
            Assert.Contains("\"Accept\": \"application/xml\"", code);
            Assert.Contains("const content_ = body;", code);
        }

        [Fact]
        public async Task WhenSpecContainsFormDataInSingleMultipartFile_ThenFormDataIsUsedInTypeScript()
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
            Assert.Contains("interface FileParameter", code);
            Assert.Contains("content_.append(\"file\", file.data, file.fileName ? file.fileName : \"file\");", code);
        }


        [Fact]
        public async Task WhenSpecContainsFormDataInMultipartFileArray_ThenFormDataIsUsedInTypeScript()
        {
            var json = @"{
  ""x-generator"": ""NSwag v13.5.0.0 (NJsonSchema v10.1.15.0 (Newtonsoft.Json v11.0.0.0))"",
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""title"": ""My Title"",
    ""version"": ""1.0.0""
  },
  ""paths"": {
    ""/api/FileUpload/UploadFiles"": {
      ""post"": {
        ""tags"": [
          ""FileUpload""
        ],
        ""operationId"": ""FileUpload_UploadFiles"",
        ""requestBody"": {
          ""content"": {
            ""multipart/form-data"": {
              ""schema"": {
                ""properties"": {
                  ""files"": {
                    ""type"": ""array"",
                    ""items"": {
                      ""type"": ""string"",
                      ""format"": ""binary""
                    }
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
            Assert.Contains("interface FileParameter", code);
            Assert.Contains("content_.append(\"files\", item_.data, item_.fileName ? item_.fileName : \"files\")", code);
        }

        [Fact]
        public async Task WhenSpecContainsFormDataInNestedMultipartForm_ThenFormDataIsUsedInTypeScript()
        {
            var json = @"{
  ""x-generator"": ""NSwag v13.5.0.0 (NJsonSchema v10.1.15.0 (Newtonsoft.Json v11.0.0.0))"",
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""title"": ""My Title"",
    ""version"": ""1.0.0""
  },
  ""paths"": {
    ""/api/FileUpload/UploadAttachment"": {
      ""post"": {
        ""tags"": [
          ""FileUpload""
        ],
        ""operationId"": ""FileUpload_UploadAttachment"",
        ""parameters"": [
          {
            ""name"": ""caseId"",
            ""in"": ""path"",
            ""required"": true,
            ""schema"": {
              ""type"": ""string"",
              ""nullable"": true
            },
            ""x-position"": 1
          }
        ],
        ""requestBody"": {
          ""content"": {
            ""multipart/form-data"": {
              ""schema"": {
                ""properties"": {
                  ""Description"": {
                    ""type"": ""string"",
                    ""nullable"": true
                  },
                  ""Contents"": {
                    ""type"": ""string"",
                    ""format"": ""binary"",
                    ""nullable"": true
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
    }
  },
  ""components"": {}
}";

            var document = await OpenApiDocument.FromJsonAsync(json);

            // Act
            var codeGenerator = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings());
            var code = codeGenerator.GenerateFile();

            // Assert
            Assert.Contains("const content_ = new FormData();", code);
            Assert.Contains("interface FileParameter", code);
            Assert.Contains("content_.append(\"Contents\", contents.data, contents.fileName ? contents.fileName : \"Contents\");", code);
        }

    }
}
