using NJsonSchema;
using Xunit;

namespace NSwag.CodeGeneration.CSharp.Tests
{
    public class ParameterTests
    {
        [Fact]
        public void When_parameters_have_same_name_then_they_are_renamed()
        {
            //// Arrange
            var document = new OpenApiDocument();
            document.Paths["foo"] = new OpenApiPathItem
            {
                {
                    OpenApiOperationMethod.Get, new OpenApiOperation
                    {
                        Parameters =
                        {
                            new OpenApiParameter
                            {
                                Kind = OpenApiParameterKind.Query,
                                Name = "foo"
                            },
                            new OpenApiParameter
                            {
                                Kind = OpenApiParameterKind.Header,
                                Name = "foo"
                            },
                        }
                    }
                }
            };

            //// Act
            var generator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings());
            var code = generator.GenerateFile();

            //// Assert
            Assert.Contains("FooAsync(object fooQuery, object fooHeader, System.Threading.CancellationToken cancellationToken)", code);
        }

        [Fact]
        public void When_parent_parameters_have_same_kind_then_they_are_included()
        {
            //// Arrange
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

""parameters"": [
                {
                ""name"": ""SecureToken"",
                    ""in"": ""header"",
                    ""description"": ""cookie"",
                    ""required"": true,
                    ""type"": ""string""
                }
            ],

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
            var document = OpenApiDocument.FromJsonAsync(swagger).Result;

            //// Act
            var generator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings());
            var code = generator.GenerateFile();

            //// Assert
            Assert.Contains("RemoveElementAsync(string x_User, System.Collections.Generic.IEnumerable<long> elementId, string secureToken)", code);          
        }

        [Fact]
        public void When_swagger_contains_optional_parameters_then_they_are_rendered_in_CSharp()
        {
            //// Arrange
            var swagger = @"{
   ""paths"":{
      ""/journeys"":{
         ""get"":{
            ""tags"":[
               ""CheckIn""
            ],
            ""summary"":""Retrieve journeys"",
            ""operationId"":""retrieveJourneys"",
            ""description"":"""",
            ""parameters"":[
               {
                  ""$ref"":""#/parameters/lastName""
               },
               {
                  ""$ref"":""#/parameters/optionalOrderId""
               },
               {
                  ""$ref"":""#/parameters/eTicketNumber""
               },
               {
                  ""$ref"":""#/parameters/optionalFrequentFlyerCardId""
               },
               {
                  ""$ref"":""#/parameters/optionalDepartureDate""
               },
               {
                  ""$ref"":""#/parameters/optionalOriginLocationCode""
               }
            ],
            ""responses"": {}
         }
      }
   },
   ""parameters"":{
      ""lastName"":{
         ""name"":""lastName"",
         ""type"":""string"",
         ""required"":true
      },
      ""optionalOrderId"":{
         ""name"":""optionalOrderId"",
         ""type"":""string"",
         ""required"":false
      },
      ""eTicketNumber"":{
         ""name"":""eTicketNumber"",
         ""type"":""string"",
         ""required"":false
      },
      ""optionalFrequentFlyerCardId"":{
         ""name"":""optionalFrequentFlyerCardId"",
         ""type"":""string"",
         ""required"":false
      },
      ""optionalDepartureDate"":{
         ""name"":""optionalDepartureDate"",
         ""type"":""string"",
         ""required"":false
      },
      ""optionalOriginLocationCode"":{
         ""name"":""optionalOriginLocationCode"",
         ""type"":""string"",
         ""required"":false
      }
   }
}";

            var document = OpenApiDocument.FromJsonAsync(swagger).Result;

            //// Act
            var generator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings());
            var code = generator.GenerateFile();

            //// Assert
            Assert.Contains("lastName", code);
            Assert.Contains("optionalOrderId", code);
        }

        [Fact]
        public void Deep_object_properties_are_correctly_named()
        {
            //// Arrange
            var swagger = @"{
   'openapi' : '3.0',
   'info' : {
      'version' : '1.0.2',
       'title' : 'Test API'
   },
   'paths': {
      '/journeys': {
         'get': {
            'tags': [
               'CheckIn'
            ],
            'summary': 'Retrieve journeys',
            'operationId': 'retrieveJourneys',
            'description': '',
            'parameters': [
                {
                   'name': 'lastName',
                   'in': 'query',
                   'schema': { 'type': 'string' }
                },
                {
                   'name': 'eTicketNumber',
                   'in': 'query',
                   'schema': { 'type': 'string' }
                },
                {
                   'name': 'options',
                   'in': 'query',
                   'style': 'deepObject',
                   'explode': true,
                   'schema': { '$ref': '#/components/schemas/Options' }
                }
            ],
            'responses': {}
         }
      }
   },
   'components':{
      'schemas': {
         'Options': {
            'type':'object',
            'properties': {
               'optionalOrder.id': {
                  'schema': { 'type': 'string' }
               },
               'optionalFrequentFlyerCard.id':{
                  'schema': { 'type': 'string' }
               },
               'optionalDepartureDate':{
                  'schema': { 'type': 'string' }
               },
               'optionalOriginLocationCode':{
                  'schema': { 'type': 'string' }
               }
            }
         }
      }
   }
}";

            var document = OpenApiDocument.FromJsonAsync(swagger, "", SchemaType.OpenApi3).Result;

            //// Act
            var generator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings());
            var code = generator.GenerateFile();

            //// Assert
            Assert.Contains(@"""options[optionalOrder.id]"") + ""=""", code);
            Assert.Contains("options.OptionalOrderId", code);
        }

        [Theory]
        [InlineData(Models.CSharpControllerStyle.Abstract)]
        [InlineData(Models.CSharpControllerStyle.Partial)]
        public void When_file_parameter_with_AspNetCore_controller_use_IFormFile(Models.CSharpControllerStyle controllerStyle)
        {
            //// Arrange
            var document = new OpenApiDocument();
            document.Paths["foo"] = new OpenApiPathItem
            {
                {
                    OpenApiOperationMethod.Get, new OpenApiOperation
                    {
                        Parameters =
                        {
                            new OpenApiParameter
                            {
                                Kind = OpenApiParameterKind.FormData,
                                Name = "foo",
                                Type = JsonObjectType.File
                            }
                        }
                    }
                }
            };

            //// Act
            var generator = new CSharpControllerGenerator(document, new CSharpControllerGeneratorSettings
            {
                ControllerStyle = controllerStyle,
                ControllerTarget = Models.CSharpControllerTarget.AspNetCore
            });
            var code = generator.GenerateFile();

            //// Assert
            Assert.Contains("System.Threading.Tasks.Task Foo(Microsoft.AspNetCore.Http.IFormFile foo)", code);
        }

        [Theory]
        [InlineData(Models.CSharpControllerStyle.Abstract)]
        [InlineData(Models.CSharpControllerStyle.Partial)]
        public void When_multiple_file_parameter_with_AspNetCore_controller_use_IFormFile(Models.CSharpControllerStyle controllerStyle)
        {
            //// Arrange
            var document = new OpenApiDocument();
            document.Paths["foo"] = new OpenApiPathItem
            {
                {
                    OpenApiOperationMethod.Get, new OpenApiOperation
                    {
                        Parameters =
                        {
                            new OpenApiParameter
                            {
                                Kind = OpenApiParameterKind.FormData,
                                Name = "foo",
                                Type = JsonObjectType.File,
                                CollectionFormat = OpenApiParameterCollectionFormat.Multi
                            }
                        }
                    }
                }
            };

            //// Act
            var generator = new CSharpControllerGenerator(document, new CSharpControllerGeneratorSettings
            {
                ControllerStyle = controllerStyle,
                ControllerTarget = Models.CSharpControllerTarget.AspNetCore
            });
            var code = generator.GenerateFile();

            //// Assert
            Assert.Contains("System.Threading.Tasks.Task Foo(System.Collections.Generic.IEnumerable<Microsoft.AspNetCore.Http.IFormFile> foo)", code);
        }
    }
}
