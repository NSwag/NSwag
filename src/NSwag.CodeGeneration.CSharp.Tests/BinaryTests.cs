using NSwag.CodeGeneration.CSharp.Models;
using System.Threading.Tasks;
using Xunit;

namespace NSwag.CodeGeneration.CSharp.Tests
{
    public class BinaryTests
    {
        [Fact]
        public async Task When_body_is_binary_then_stream_is_used_as_parameter_in_CSharp()
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
            application/json:
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
            var codeGenerator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings());
            var code = codeGenerator.GenerateFile();

            // Assert
            Assert.Contains("public async System.Threading.Tasks.Task<FileToken> AddFileAsync(System.IO.Stream body, System.Threading.CancellationToken cancellationToken)", code);
            Assert.Contains("var content_ = new System.Net.Http.StreamContent(body);", code);
        }

        [Fact]
        public async Task When_body_is_binary_then_IFormFile_is_used_as_parameter_in_CSharp_ASPNETCore()
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
          description: 'something'
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/FileToken'
      requestBody:
       content:
         multipart/form-data:
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
            CSharpControllerGeneratorSettings settings = new CSharpControllerGeneratorSettings();
            settings.ControllerTarget = CSharpControllerTarget.AspNetCore;
            var codeGenerator = new CSharpControllerGenerator(document, settings);
            var code = codeGenerator.GenerateFile();

            // Assert
            Assert.Contains("Microsoft.AspNetCore.Http.IFormFile body", code);
            Assert.DoesNotContain("FromBody]", code);
        }

        [Fact]
        public async Task When_body_is_binary_array_then_IFormFile_Collection_is_used_as_parameter_in_CSharp_ASPNETCore()
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
          description: 'something'
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/FileToken'
      requestBody:
       content:
         multipart/form-data:
           schema:
             type: array
             items:
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
            CSharpControllerGeneratorSettings settings = new CSharpControllerGeneratorSettings();
            settings.ControllerTarget = CSharpControllerTarget.AspNetCore;
            var codeGenerator = new CSharpControllerGenerator(document, settings);
            var code = codeGenerator.GenerateFile();

            // Assert
            Assert.Contains("System.Collections.Generic.ICollection<Microsoft.AspNetCore.Http.IFormFile> body", code);
            Assert.DoesNotContain("FromBody]", code);
        }

        [Fact]
        public async Task WhenSpecContainsFormDataInSingleMultipartFile_ThenFormDataIsUsedInCSharp()
        {
            var json = @"{
  ""x-generator"": ""NSwag v13.7.0.0 (NJsonSchema v10.2.0.0 (Newtonsoft.Json v11.0.0.0))"",
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
                ""type"": ""object"",
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
    }
  },
  ""components"": {}
}";

            var document = await OpenApiDocument.FromJsonAsync(json);

            // Act
            var codeGenerator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings());
            var code = codeGenerator.GenerateFile();

            // Assert
            Assert.Contains("var content_ = new System.Net.Http.MultipartFormDataContent(boundary_);", code);
            Assert.Contains("var content_file_ = new System.Net.Http.StreamContent(file.Data);", code);
            Assert.Contains("class FileParameter", code);
            Assert.Contains("content_.Add(content_file_, \"file\", file.FileName ?? \"file\");", code);
        }

        [Fact]
        public async Task WhenSpecContainsFormDataInMultipartFileArray_ThenFormDataIsUsedInCSharp()
        {
            var json = @"{
  ""x-generator"": ""NSwag v13.7.0.0 (NJsonSchema v10.2.0.0 (Newtonsoft.Json v11.0.0.0))"",
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
                ""type"": ""object"",
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
    }
  },
  ""components"": {}
}";

            var document = await OpenApiDocument.FromJsonAsync(json);

            // Act
            var codeGenerator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings());
            var code = codeGenerator.GenerateFile();

            // Assert
            Assert.Contains("var content_ = new System.Net.Http.MultipartFormDataContent(boundary_);", code);
            Assert.Contains("var content_files_ = new System.Net.Http.StreamContent(item_.Data);", code);
            Assert.Contains("class FileParameter", code);
            Assert.Contains("content_.Add(content_files_, \"files\", item_.FileName ?? \"files\");", code);
        }

        [Fact]
        public async Task WhenSpecContainsFormDataInNestedMultipartForm_ThenFormDataIsUsedInCSharp()
        {
            var json = @"{
  ""x-generator"": ""NSwag v13.7.0.0 (NJsonSchema v10.2.0.0 (Newtonsoft.Json v11.0.0.0))"",
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
                ""type"": ""object"",
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
            var codeGenerator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings());
            var code = codeGenerator.GenerateFile();

            // Assert
            Assert.Contains("var content_ = new System.Net.Http.MultipartFormDataContent(boundary_);", code);
            Assert.Contains("var content_contents_ = new System.Net.Http.StreamContent(contents.Data);", code);
            Assert.Contains("class FileParameter", code);
            Assert.Contains("content_.Add(content_contents_, \"Contents\", contents.FileName ?? \"Contents\");", code);
        }

    }
}
