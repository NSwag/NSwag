using System.Threading.Tasks;
using Xunit;

namespace NSwag.CodeGeneration.CSharp.Tests
{
    public class ResponseTests
    {
        [Fact]
        public async Task When_response_references_object_in_inheritance_hierarchy_then_return_value_is_correct()
        {
            var json = @"{
  ""openapi"": ""3.0.0"",
  ""paths"": {
    ""/v1/exceptions/get"": {
      ""post"": {
        ""operationId"": ""Exceptions_GetException"",
        ""responses"": {
          ""200"": {
            ""content"": {
              ""application/json"": {
                ""schema"": {
                  ""$ref"": ""#/components/schemas/ValidationException""
                }
              }
            }
          }
        }
      }
    }
  }, 
  ""components"": {
    ""schemas"": {
      ""BusinessException"": {
        ""type"": ""object"",
        ""additionalProperties"": false,
        ""properties"": {
          ""customerId"": {
            ""type"": ""string"",
            ""nullable"": true
          },
          ""customerAlias"": {
            ""type"": ""string"",
            ""nullable"": true
          },
          ""userId"": {
            ""type"": ""string"",
            ""nullable"": true
          }
        }
      },
      ""ValidationException"": {
        ""allOf"": [
          {
            ""$ref"": ""#/components/schemas/BusinessException""
          },
          {
            ""type"": ""object"",
            ""additionalProperties"": false
          }
        ]
      }
    }
  }
}";
            var document = await OpenApiDocument.FromJsonAsync(json);

            // Act
            var settings = new CSharpClientGeneratorSettings { ClassName = "MyClass" };
            var generator = new CSharpClientGenerator(document, settings);
            var code = generator.GenerateFile();

            // Act
            Assert.Contains("System.Threading.Tasks.Task<ValidationException>", code);
            Assert.DoesNotContain("System.Threading.Tasks.Task<object>", code);
            Assert.Contains("class BusinessException", code);
            Assert.Contains("class ValidationException", code);
        }

        [Fact]
        public async Task When_response_is_referenced_any_then_class_is_generated()
        {
            var json = @"{
  ""openapi"": ""3.0.0"",
  ""paths"": {
    ""/v1/exceptions/get"": {
      ""post"": {
        ""operationId"": ""Exceptions_GetException"",
        ""responses"": {
          ""200"": {
            ""content"": {
              ""application/json"": {
                ""schema"": {
                  ""$ref"": ""#/components/schemas/BusinessException""
                }
              }
            }
          }
        }
      }
    }
  }, 
  ""components"": {
    ""schemas"": {
      ""BusinessException"": {
        ""type"": ""object"",
        ""additionalProperties"": false
      }
    }
  }
}";
            var document = await OpenApiDocument.FromJsonAsync(json);

            // Act
            var settings = new CSharpClientGeneratorSettings { ClassName = "MyClass" };
            var generator = new CSharpClientGenerator(document, settings);
            var code = generator.GenerateFile();

            // Act
            Assert.Contains("System.Threading.Tasks.Task<BusinessException>", code);
            Assert.DoesNotContain("System.Threading.Tasks.Task<object>", code);
            Assert.Contains("class BusinessException", code);
        }

        [Fact]
        public async Task When_same_response_is_referenced_multiple_times_in_operation_then_class_is_generated()
        {
            string json = @"{
  ""openapi"": ""3.0.0"",
  ""paths"": {
    ""/v1/exceptions/get"": {
      ""get"": {
        ""operationId"": ""Exceptions_GetException"",
        ""responses"": {
          ""200"": {
            ""$ref"": ""#/components/responses/BusinessExceptionResponse"",
            },
          ""400"": {
            ""$ref"": ""#/components/responses/BusinessExceptionResponse"",
          }
        }
      }
    }
  },
  ""components"": {
    ""schemas"": {
      ""BusinessException"": {
        ""type"": ""object"",
        ""additionalProperties"": false
      }
    },
    ""responses"": {
      ""BusinessExceptionResponse"": {
        ""description"": ""List of NSwagStudio bugs"",
        ""content"": {
          ""application/json"": {
            ""schema"": {
              ""type"": ""array"",
              ""items"": {
                ""$ref"": ""#/components/schemas/BusinessException""
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
            var settings = new CSharpClientGeneratorSettings { ClassName = "MyClass" };
            var generator = new CSharpClientGenerator(document, settings);
            var code = generator.GenerateFile();

            // Act
            Assert.Contains("System.Threading.Tasks.Task<System.Collections.Generic.ICollection<BusinessException>>", code);
            Assert.Contains("class BusinessException", code);
        }
    }
}
