using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NJsonSchema;
using Xunit;

namespace NSwag.Core.Tests
{
    public class DocumentLoadingTests
    {
        [Fact]
        public async Task When_document_contains_readOnly_properties_then_they_are_correctly_loaded()
        {
            // Arrange
            var json = _sampleServiceCode;

            // Act
            var document = await OpenApiDocument.FromJsonAsync(json);
            var json2 = document.ToJson();
            var reference = document.Paths["/pets"][OpenApiOperationMethod.Get].ActualResponses["200"].Schema.Item.Reference;

            // Assert
            Assert.NotNull(json2);
            Assert.NotNull(reference);
            Assert.Equal(3, reference.Properties.Count);
            Assert.True(document.Definitions["Pet"].Properties["id"].IsReadOnly);
            Assert.DoesNotContain(@"""readonly""", json2);
            Assert.Contains(@"""readOnly""", json2);
        }

        [Fact]
        public async Task When_generating_operation_ids_then_missing_ids_are_generated()
        {
            // Arrange
            var json = _sampleServiceCode;

            // Act
            var document = await OpenApiDocument.FromJsonAsync(json);
            document.GenerateOperationIds();

            // Assert
            Assert.Equal("pets", document.Operations.First().Operation.OperationId);
        }

        [Fact]
        public async Task When_json_has_extension_data_then_it_is_loaded()
        {
            // Arrange
            var json = _jsonVendorExtensionData;

            // Act
            var document = await OpenApiDocument.FromJsonAsync(json);

            // Assert
            Assert.NotNull(document.Operations.First().Operation.ActualResponses["202"].ExtensionData);
        }

        [Fact]
        public async Task When_locale_is_not_english_then_types_are_correctly_serialized()
        {
            // https://github.com/RicoSuter/NSwag/issues/518

            // Arrange
            CultureInfo ci = new CultureInfo("tr-TR");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
            CultureInfo.DefaultThreadCurrentCulture = ci;

            // Act
            var json = _sampleServiceCode;

            // Act
            var document = await OpenApiDocument.FromJsonAsync(json);
            var j = document.ToJson();

            // Assert
            Assert.Equal(JsonObjectType.Integer, document.Definitions["Pet"].Properties["id"].Type);
        }

        private string _sampleServiceCode =
@"{
  ""swagger"": ""2.0"",
  ""info"": {
    ""version"": ""1.0.0"",
    ""title"": ""Swagger Petstore"",
    ""description"": ""A sample API that uses a petstore as an example to demonstrate features in the swagger-2.0 specification"",
    ""termsOfService"": ""http://swagger.io/terms/""
  },
  ""host"": ""petstore.swagger.io"",
  ""basePath"": ""/api"",
  ""schemes"": [
    ""http""
  ],
  ""consumes"": [
    ""application/json""
  ],
  ""produces"": [
    ""application/json""
  ],
  ""paths"": {
    ""/pets"": {
      ""get"": {
        ""description"": ""Returns all pets from the system that the user has access to"",
        ""produces"": [
          ""application/json""
        ],
        ""responses"": {
          ""200"": {
            ""description"": ""A list of pets."",
            ""schema"": {
              ""type"": ""array"",
              ""items"": {
                ""$ref"": ""#/definitions/Pet""
              }
            }
          }
        }
      }
    }
  },
  ""definitions"": {
    ""Pet"": {
      ""type"": ""object"",
      ""required"": [
        ""id"",
        ""name""
      ],
      ""properties"": {
        ""id"": {
          ""type"": ""integer"",
          ""format"": ""int64"",
          ""readOnly"": ""true""
        },
        ""name"": {
          ""type"": ""string""
        },
        ""tag"": {
          ""type"": ""string""
        }
      }
    }
  }
}";

        private string _jsonVendorExtensionData =
                    @"{
  ""swagger"": ""2.0"",
  ""info"": {
    ""title"": ""Swagger Test Sample"",
    ""description"": ""Swagger Test"",
    ""version"": ""1.0.0""
  },
  ""schemes"": [
    ""https""
  ],
  ""basePath"": ""/api/v1"",
  ""produces"": [
    ""application/json""
  ],
  ""consumes"": [
    ""application/json""
  ],
  ""host"": ""test.com"",
  ""paths"": {
    ""/12345/instances"": {
      ""post"": {
        ""summary"": ""Starts operation"",
        ""description"": ""Starts operation to trigger a task"",
        ""operationId"": ""123"",
        ""parameters"": [
          {
            ""name"": ""API Parameters"",
            ""required"": true,
            ""in"": ""body"",
            ""schema"": {
              ""type"": ""object"",
              ""properties"": {
                ""data"": {
                  ""type"": ""object"",
                  ""properties"": {
                    ""prop1"": {
                      ""title"": ""title 1"",
                      ""description"": ""description 1"",
                      ""type"": ""string""
                    },
                    ""prop2"": {
                      ""title"": ""title 2"",
                      ""description"": ""descripiton 2"",
                      ""type"": ""string""
                    }
                  }
                },
                ""options"": {
                  ""type"": ""object"",
                  ""properties"": {
                    ""callbackUrl"": {
                      ""title"": ""callbackUrl"",
                      ""description"": ""A Url to return the results back"",
                      ""type"": ""string""
                    }
                  }
                }
              }
            }
          },
          {
            ""name"": ""token"",
            ""type"": ""string"",
            ""in"": ""query"",
            ""description"": ""A security token""
          }
        ],
        ""responses"": {
          ""202"": {
            ""description"": ""Accepted"",
            ""x-callback-schema"": {
              ""type"": ""object"",
              ""properties"": {
                ""returnData"": {
                  ""type"": ""object"",
                  ""properties"": {
                    ""prop1"": {
                      ""title"": ""title 1"",
                      ""description"": ""description 1"",
                      ""type"": ""string""
                    },
                    ""prop2"": {
                      ""title"": ""title 2"",
                      ""description"": ""descripiton 2"",
                      ""type"": ""string""
                    }
                  }
                },
                ""workflow"": {
                  ""type"": ""object"",
                  ""properties"": {
                    ""id"": ""123"",
                    ""name"": ""Swagger Test""
                  }
                }
              }
            }
          },
          ""400"": {
            ""description"": ""Bad Request""
          },
          ""404"": {
            ""description"": ""Not Found""
          },
          ""429"": {
            ""description"": ""Too Many Requests""
          },
          ""503"": {
            ""description"": ""Service Unavailable - Overloaded""
          },
          ""default"": {
            ""description"": ""Unexpected Error""
          }
        }
      }
    }
  }
}";
    }
}
