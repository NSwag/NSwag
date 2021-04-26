//-----------------------------------------------------------------------
// <copyright file="AspNetCoreToSwaggerGenerator.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;
using NSwag.Annotations;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using Xunit;

namespace NSwag.Generation.AspNetCore.Tests
{
    public class AspNetCoreToSwaggerGenerationTests
    {
        [Fact]
        public async Task SwaggerDocumentIsGeneratedForCustomCreatedApiDescriptions()
        {
            // Arrange
            var generator = new AspNetCoreOpenApiDocumentGenerator(new AspNetCoreOpenApiDocumentGeneratorSettings());
            var apiDescriptions = new[]
            {
                new ApiDescription
                {
                    ActionDescriptor = new ActionDescriptor(),
                },
                new ApiDescription
                {
                    ActionDescriptor = new ControllerActionDescriptor
                    {
                        ControllerTypeInfo = typeof(CustomController).GetTypeInfo(),
                        MethodInfo = typeof(CustomController).GetMethod(nameof(CustomController.FindModel)),
                        AttributeRouteInfo = new AttributeRouteInfo
                        {
                            Template = "api/test/{id}",
                        },
                        ControllerName = "Test",
                        ActionName = "Find",
                    },
                    HttpMethod = "Get",
                    RelativePath = "api/test/{id}",
                    ParameterDescriptions =
                    {
                        new ApiParameterDescription
                        {
                            Name = "id",
                            Source = BindingSource.Path,
                            Type = typeof(int),
                        },
                    },
                    SupportedResponseTypes =
                    {
                        new ApiResponseType
                        {
                            Type = typeof(TestModel),
                            StatusCode = 200,
                        }
                    },
                },
                new ApiDescription
                {
                    ActionDescriptor = new ControllerActionDescriptor
                    {
                        ControllerTypeInfo = typeof(CustomController).GetTypeInfo(),
                        MethodInfo = typeof(CustomController).GetMethod(nameof(CustomController.Delete)),
                        ControllerName = "Test",
                        ActionName = "DeleteModel",
                    },
                    HttpMethod = "Delete",
                    RelativePath = "api/test",
                    ParameterDescriptions =
                    {
                        new ApiParameterDescription
                        {
                            Name = "id",
                            Source = BindingSource.Query,
                            Type = typeof(int),
                        },
                    },
                    SupportedResponseTypes =
                    {
                        new ApiResponseType
                        {
                            Type = typeof(StatusCodeResult),
                            StatusCode = 201,
                        }
                    },
                },
                new ApiDescription
                {
                    ActionDescriptor = new ControllerActionDescriptor
                    {
                        ControllerTypeInfo = typeof(CustomController).GetTypeInfo(),
                        MethodInfo = typeof(CustomController).GetMethod(nameof(CustomController.Update)),
                        AttributeRouteInfo = new AttributeRouteInfo
                        {
                            Template = "api/test/{id}",
                        },
                        ControllerName = "Test",
                        ActionName = "Update",
                    },
                    HttpMethod = "Put",
                    RelativePath = "api/test/{id}",
                    ParameterDescriptions =
                    {
                        new ApiParameterDescription
                        {
                            Type = typeof(int),
                            Name = "id",
                            Source = BindingSource.Path,
                        },
                        new ApiParameterDescription
                        {
                            Type = typeof(TestModel),
                            Name = "model",
                            Source = BindingSource.Body,
                        },
                    },
                    SupportedResponseTypes =
                    {
                        new ApiResponseType
                        {
                            Type = typeof(Task<TestModel>),
                            StatusCode = 200,
                        }
                    },
                },
            };
            var apiDescriptionGroupCollection = new ApiDescriptionGroupCollection(
                new[] { new ApiDescriptionGroup(string.Empty, apiDescriptions) },
                version: 1);

            // Act
            var document = await generator.GenerateAsync(apiDescriptionGroupCollection);

            // Assert
            Assert.Collection(
                document.Operations.OrderBy(o => o.Method.ToString()),
                operation =>
                {
                    Assert.Equal("/api/test", operation.Path);
                    Assert.Equal(OpenApiOperationMethod.Delete, operation.Method);
                },
                operation =>
                {
                    Assert.Equal("/api/test/{id}", operation.Path);
                    Assert.Equal(OpenApiOperationMethod.Get, operation.Method);

                    Assert.Single(operation.Operation.Responses);
                    var response = operation.Operation.Responses["200"];
                    var definition = document.Definitions.First(f => f.Value == response.Schema?.ActualSchema);
                    Assert.Equal(nameof(TestModel), definition.Key);
                },
                operation =>
                {
                    Assert.Equal("/api/test/{id}", operation.Path);
                    Assert.Equal(OpenApiOperationMethod.Put, operation.Method);
                });
        }

        [Fact]
        public async Task When_generating_swagger_all_apidescriptions_are_discovered()
        {
            // Arrange
            var generator = new AspNetCoreOpenApiDocumentGenerator(new AspNetCoreOpenApiDocumentGeneratorSettings());
            var apiDescriptions = GetApiDescriptionGroups(typeof(TestController));

            // Act
            var document = await generator.GenerateAsync(apiDescriptions);

            // Assert
            var operationDescription = Assert.Single(document.Operations);
            Assert.Equal("/test", operationDescription.Path);
            Assert.Equal(OpenApiOperationMethod.Get, operationDescription.Method);

            var operation = operationDescription.Operation;
            Assert.Equal("Test_FindModel", operation.OperationId);

            var parameter = Assert.Single(operation.Parameters);
            Assert.Equal("id", parameter.Name);
            Assert.Equal(OpenApiParameterKind.Path, parameter.Kind);
            Assert.True(parameter.IsRequired);
            Assert.Equal(NJsonSchema.JsonObjectType.Integer, parameter.Type);

            Assert.Single(operation.Responses);
            var response = operation.Responses["200"];
            var definition = document.Definitions.First(f => f.Value == response.Schema?.ActualSchema);
            Assert.Equal(nameof(TestModel), definition.Key);
        }

        [Fact]
        public async Task When_generating_swagger_all_apidescriptions_are_discovered_for_2_1_applications()
        {
            // Arrange
            var generator = new AspNetCoreOpenApiDocumentGenerator(new AspNetCoreOpenApiDocumentGeneratorSettings());
            var apiDescriptions = Get2_1_ApiDescriptionGroups(typeof(TestController));

            // Act
            var document = await generator.GenerateAsync(apiDescriptions);

            // Assert
            var operationDescription = Assert.Single(document.Operations);
            Assert.Equal("/test", operationDescription.Path);
            Assert.Equal(OpenApiOperationMethod.Get, operationDescription.Method);

            var operation = operationDescription.Operation;
            Assert.Equal("Test_FindModel", operation.OperationId);

            var parameter = Assert.Single(operation.Parameters);
            Assert.Equal("id", parameter.Name);
            Assert.Equal(OpenApiParameterKind.Path, parameter.Kind);
            Assert.True(parameter.IsRequired);
            Assert.Equal(NJsonSchema.JsonObjectType.Integer, parameter.Type);

            Assert.Equal(2, operation.Responses.Count);
            var response = operation.Responses["200"];
            var definition = document.Definitions.First(f => f.Value == response.Schema?.ActualSchema);
            Assert.Equal(nameof(TestModel), definition.Key);

            response = operation.Responses["default"];
            definition = document.Definitions.First(f => f.Value == response.Schema?.ActualSchema);
            Assert.Equal(nameof(ProblemDetails), definition.Key);
        }

        [Fact]
        public async Task ControllersWithSwaggerIgnoreAttribute_AreIgnored()
        {
            // Arrange
            var generator = new AspNetCoreOpenApiDocumentGenerator(new AspNetCoreOpenApiDocumentGeneratorSettings());
            var apiDescriptions = GetApiDescriptionGroups(typeof(ControllerWithSwaggerIgnoreAttribute));

            // Act
            var document = await generator.GenerateAsync(apiDescriptions);

            // Assert
            Assert.Empty(document.Operations);
        }

        [Fact]
        public async Task ActionsWithSwaggerIgnoreAttribute_AreIgnored()
        {
            // Arrange
            var generator = new AspNetCoreOpenApiDocumentGenerator(new AspNetCoreOpenApiDocumentGeneratorSettings());
            var apiDescriptions = GetApiDescriptionGroups(typeof(ActionWithSwaggerIgnoreAttribute));

            // Act
            var document = await generator.GenerateAsync(apiDescriptions);

            // Assert
            var operationDescription = Assert.Single(document.Operations);
            Assert.Equal("/test1", operationDescription.Path);
            Assert.Equal(OpenApiOperationMethod.Get, operationDescription.Method);
        }

        [Fact]
        public async Task ParametersWithSwaggerIgnoreAttribute_AreIgnored()
        {
            // Arrange
            var generator = new AspNetCoreOpenApiDocumentGenerator(new AspNetCoreOpenApiDocumentGeneratorSettings());
            var apiDescriptions = GetApiDescriptionGroups(typeof(ParameterWithSwaggerIgnoreAttribute));

            // Act
            var document = await generator.GenerateAsync(apiDescriptions);

            // Assert
            var operationDescription = Assert.Single(document.Operations);
            Assert.Equal("/{id}", operationDescription.Path);
            Assert.Equal(OpenApiOperationMethod.Post, operationDescription.Method);

            var parameter = Assert.Single(operationDescription.Operation.Parameters);
            Assert.Equal("id", parameter.Name);
            Assert.Equal(OpenApiParameterKind.Path, parameter.Kind);
        }

        [Fact]
        public async Task SwaggerOperationMethods_AreParsed()
        {
            // Arrange
            var generator = new AspNetCoreOpenApiDocumentGenerator(new AspNetCoreOpenApiDocumentGeneratorSettings());
            var apiDescriptions = GetApiDescriptionGroups(typeof(HttpMethodsController));

            // Act
            var document = await generator.GenerateAsync(apiDescriptions);

            // Assert
            Assert.Collection(
                document.Operations.OrderBy(o => o.Method.ToString()),
                operation =>
                {
                    Assert.Equal(OpenApiOperationMethod.Delete, operation.Method);
                    Assert.Equal("HttpMethods_HttpDelete", operation.Operation.OperationId);
                },
                operation =>
                {
                    Assert.Equal(OpenApiOperationMethod.Head, operation.Method);
                    Assert.Equal("HttpMethods_HttpHead", operation.Operation.OperationId);
                },
                operation =>
                {
                    Assert.Equal(OpenApiOperationMethod.Options, operation.Method);
                    Assert.Equal("HttpMethods_HttpOptions", operation.Operation.OperationId);
                },
                operation =>
                {
                    Assert.Equal(OpenApiOperationMethod.Patch, operation.Method);
                    Assert.Equal("HttpMethods_HttpPatch", operation.Operation.OperationId);
                });
        }

        [Fact]
        public async Task SwaggerOperationAttribute_AreUsedToCalculateOperationId_IfPresent()
        {
            // Arrange
            var generator = new AspNetCoreOpenApiDocumentGenerator(new AspNetCoreOpenApiDocumentGeneratorSettings());
            var apiDescriptions = GetApiDescriptionGroups(typeof(ActionWithSwaggerOperationAttribute));

            // Act
            var document = await generator.GenerateAsync(apiDescriptions);

            // Assert
            var operation = Assert.Single(document.Operations);
            Assert.Equal("CustomOperationId", operation.Operation.OperationId);
        }

        [Fact]
        public async Task SwaggerOperationProcessorAttributesOnControllerTypes_AreDiscoveredAndExecuted()
        {
            // Arrange
            var generator = new AspNetCoreOpenApiDocumentGenerator(new AspNetCoreOpenApiDocumentGeneratorSettings());
            var apiDescriptions = GetApiDescriptionGroups(typeof(ControllerWithSwaggerOperationProcessor));

            // Act
            var document = await generator.GenerateAsync(apiDescriptions);

            // Assert
            Assert.Equal("Hello from controller", document.Info.Title);
        }

        [Fact]
        public async Task SwaggerOperationProcessorAttributesOnActions_AreDiscoveredAndExecuted()
        {
            // Arrange
            var generator = new AspNetCoreOpenApiDocumentGenerator(new AspNetCoreOpenApiDocumentGeneratorSettings());
            var apiDescriptions = GetApiDescriptionGroups(typeof(ActionWithSwaggerOperationProcessor));

            // Act
            var document = await generator.GenerateAsync(apiDescriptions);

            // Assert
            Assert.Equal("Hello from action", document.Info.Title);
        }

        [Fact]
        public async Task SwaggerResponseAttributesOnControllersAreDiscovered()
        {
            // Arrange
            var generator = new AspNetCoreOpenApiDocumentGenerator(new AspNetCoreOpenApiDocumentGeneratorSettings());
            var apiDescriptions = GetApiDescriptionGroups(typeof(ControllerWithSwaggerResponseTypeAttribute));

            // Act
            var document = await generator.GenerateAsync(apiDescriptions);

            // Assert
            var operation = Assert.Single(document.Operations);
            Assert.Single(operation.Operation.Responses);
            var response = operation.Operation.Responses["202"];
            var definition = document.Definitions.First(f => f.Value == response.Schema?.ActualSchema);
            Assert.Equal(nameof(TestModel), definition.Key);
        }

        [Fact]
        public async Task SwaggerResponseAttributesOnActionsAreDiscovered()
        {
            // Arrange
            var generator = new AspNetCoreOpenApiDocumentGenerator(new AspNetCoreOpenApiDocumentGeneratorSettings());
            var apiDescriptions = GetApiDescriptionGroups(typeof(ActionWithSwaggerResponseAttribute));

            // Act
            var document = await generator.GenerateAsync(apiDescriptions);

            // Assert
            var operation = Assert.Single(document.Operations);
            Assert.Single(operation.Operation.Responses);
            var response = operation.Operation.Responses["201"];
            var definition = document.Definitions.First(f => f.Value == response.Schema?.ActualSchema);
            Assert.Equal(nameof(TestModel), definition.Key);
        }

        [Fact]
        public async Task FromHeaderParametersAreDiscovered()
        {
            // Arrange
            var generator = new AspNetCoreOpenApiDocumentGenerator(new AspNetCoreOpenApiDocumentGeneratorSettings());
            var apiDescriptions = GetApiDescriptionGroups(typeof(ControllerWithParameters));

            // Act
            var document = await generator.GenerateAsync(apiDescriptions);

            // Assert
            var operation = Assert.Single(document.Operations, o => o.Path == "/" + nameof(ControllerWithParameters.FromHeaderParameter)).Operation;
            var parameter = Assert.Single(operation.Parameters);
            Assert.Equal(OpenApiParameterKind.Header, parameter.Kind);
            Assert.Equal("headerParameter", parameter.Name);
        }

        [Fact]
        public async Task FromBodyParametersAreDiscovered()
        {
            // Arrange
            var generator = new AspNetCoreOpenApiDocumentGenerator(new AspNetCoreOpenApiDocumentGeneratorSettings());
            var apiDescriptions = GetApiDescriptionGroups(typeof(ControllerWithParameters));

            // Act
            var document = await generator.GenerateAsync(apiDescriptions);

            // Assert
            var operation = Assert.Single(document.Operations, o => o.Path == "/" + nameof(ControllerWithParameters.FromBodyParameter)).Operation;
            var parameter = Assert.Single(operation.Parameters);
            Assert.Equal(OpenApiParameterKind.Body, parameter.Kind);
            Assert.Equal("model", parameter.Name);
            Assert.True(parameter.IsRequired);
        }

        [Fact]
        public async Task FromFormParametersAreDiscovered()
        {
            // Arrange
            var generator = new AspNetCoreOpenApiDocumentGenerator(new AspNetCoreOpenApiDocumentGeneratorSettings { RequireParametersWithoutDefault = true });
            var apiDescriptions = GetApiDescriptionGroups(typeof(ControllerWithParameters));

            // Act
            var document = await generator.GenerateAsync(apiDescriptions);

            // Assert
            var operation = Assert.Single(document.Operations, o => o.Path == "/" + nameof(ControllerWithParameters.FromFormParameter)).Operation;
            var parameter = Assert.Single(operation.Parameters);
            Assert.Equal(OpenApiParameterKind.FormData, parameter.Kind);
            Assert.Equal("formParameter", parameter.Name);
            Assert.True(parameter.IsRequired);
        }

        [Fact]
        public async Task QueryParametersAreDiscovered()
        {
            // Arrange
            var generator = new AspNetCoreOpenApiDocumentGenerator(new AspNetCoreOpenApiDocumentGeneratorSettings { RequireParametersWithoutDefault = true });
            var apiDescriptions = GetApiDescriptionGroups(typeof(ControllerWithParameters));

            // Act
            var document = await generator.GenerateAsync(apiDescriptions);

            // Assert
            var operation = Assert.Single(document.Operations, o => o.Path == "/" + nameof(ControllerWithParameters.QueryParameter)).Operation;
            var requiredParameter = operation.Parameters.First();
            var optionalParameter = operation.Parameters.Last();

            Assert.Equal(OpenApiParameterKind.Query, requiredParameter.Kind);
            Assert.Equal(OpenApiParameterKind.Query, optionalParameter.Kind);

            Assert.Equal("queryParameter1", requiredParameter.Name);
            Assert.Equal("queryParameter2", optionalParameter.Name);

            Assert.True(requiredParameter.IsRequired);
            Assert.False(optionalParameter.IsRequired);
        }

        [Fact]
        public async Task FormFileParametersAreDiscovered()
        {
            // Arrange
            var generator = new AspNetCoreOpenApiDocumentGenerator(new AspNetCoreOpenApiDocumentGeneratorSettings());
            var apiDescriptions = GetApiDescriptionGroups(typeof(ControllerWithParameters));

            // Act
            var document = await generator.GenerateAsync(apiDescriptions);

            // Assert
            var operation = Assert.Single(document.Operations, o => o.Path == "/" + nameof(ControllerWithParameters.FileParameter)).Operation;
            var parameter = Assert.Single(operation.Parameters);
            Assert.Equal(OpenApiParameterKind.FormData, parameter.Kind);
            Assert.Equal("multipart/form-data", Assert.Single(operation.Consumes));
        }

        [Fact]
        public async Task ComplexQueryParametersAreProcessed()
        {
            // Arrange
            var generator = new AspNetCoreOpenApiDocumentGenerator(new AspNetCoreOpenApiDocumentGeneratorSettings());
            var apiDescriptions = GetApiDescriptionGroups(typeof(ControllerWithParameters));

            // Act
            var document = await generator.GenerateAsync(apiDescriptions);

            // Assert
            var operation = Assert.Single(document.Operations, o => o.Path == "/" + nameof(ControllerWithParameters.ComplexFromQueryParameter)).Operation;
            Assert.Collection(
                operation.Parameters.OrderBy(p => p.Name),
                parameter =>
                {
                    Assert.Equal(nameof(ComplexModel.Header), parameter.Name);
                    Assert.Equal(OpenApiParameterKind.Query, parameter.Kind);
                },
                parameter =>
                {
                    Assert.Equal(nameof(ComplexModel.Id), parameter.Name);
                    Assert.Equal(OpenApiParameterKind.Query, parameter.Kind);
                },
                parameter =>
                {
                    Assert.Equal(nameof(ComplexModel.QueryValues), parameter.Name);
                    Assert.Equal(OpenApiParameterKind.Query, parameter.Kind);
                });
        }

        [Fact]
        public async Task BoundPropertiesAreProcessed()
        {
            // Arrange
            var generator = new AspNetCoreOpenApiDocumentGenerator(new AspNetCoreOpenApiDocumentGeneratorSettings());
            var apiDescriptions = GetApiDescriptionGroups(typeof(ControllerWithBoundProperties));

            // Act
            var document = await generator.GenerateAsync(apiDescriptions);

            // Assert
            var operation = Assert.Single(document.Operations).Operation;
            Assert.Collection(
                operation.Parameters.OrderBy(p => p.Name),
                parameter =>
                {
                    Assert.Equal(nameof(ControllerWithBoundProperties.HeaderValue), parameter.Name);
                    Assert.Equal(OpenApiParameterKind.Header, parameter.Kind);
                },
                parameter =>
                {
                    Assert.Equal(nameof(ControllerWithBoundProperties.Id), parameter.Name);
                    Assert.Equal(OpenApiParameterKind.Path, parameter.Kind);
                },
                parameter =>
                {
                    Assert.Equal("model", parameter.Name);
                    Assert.Equal(OpenApiParameterKind.Body, parameter.Kind);
                });
        }

        [Fact]
        public async Task When_no_IncludedVersions_are_defined_then_all_routes_are_available_and_replaced()
        {
            // Arrange
            var generator = new AspNetCoreOpenApiDocumentGenerator(new AspNetCoreOpenApiDocumentGeneratorSettings());
            var apiDescriptions = GetApiDescriptionGroups(typeof(ControllerWithReCodeAttribute));

            // Act
            var document = await generator.GenerateAsync(apiDescriptions);

            // Assert
            var operation = Assert.Single(document.Operations).Operation;
            Assert.True(operation.ExtensionData.ContainsKey("x-code-samples"));
            var extenstionData = (IList<ReDocCodeSampleAttribute.ReDocCodeSample>)operation.ExtensionData["x-code-samples"];
            Assert.Equal(2, extenstionData.Count);
        }

        #region Controllers

        private class CustomController
        {
            public IActionResult FindModel(int id) => null;

            [HttpDelete]
            public IActionResult Delete(int id) => null;

            [HttpPut]
            public IActionResult Update(int id, TestModel model) => null;
        }

        private class TestController : ControllerBase
        {
            [HttpGet("/test")]
            public Task<TestModel> FindModel([FromRoute] int id) => null;
        }

        [SwaggerIgnore]
        private class ControllerWithSwaggerIgnoreAttribute
        {
            [HttpGet("/test")]
            public Task<TestModel> FindModel([FromRoute] int id) => null;
        }

        private class ActionWithSwaggerIgnoreAttribute
        {
            [HttpGet("/test1")]
            public Task<TestModel> PublicApi() => null;

            [SwaggerIgnore]
            [HttpGet("/test2")]
            public Task<TestModel> SecretApi([FromRoute] int id) => null;
        }

        private class ParameterWithSwaggerIgnoreAttribute
        {
            [HttpPost("{id}/{name?}")]
            public Task<TestModel> GetModel([FromRoute] int id, [FromRoute] [SwaggerIgnore] string name) => null;
        }

        [Route("[controller]/[action]")]
        public class HttpMethodsController
        {
            [AcceptVerbs("Delete")]
            public void HttpDelete() { }

            [AcceptVerbs("pAtch")]
            public void HttpPatch() { }

            [HttpHead]
            public void HttpHead() { }

            [AcceptVerbs("OPTIONS")]
            public void HttpOptions() { }
        }

        public class ActionWithSwaggerOperationAttribute
        {
            [HttpGet("/test")]
            [SwaggerOperation("CustomOperationId")]
            public void TestAction() { }
        }

        [SwaggerOperationProcessor(typeof(TestSwaggerOperationProcessor), "Hello from controller")]
        public class ControllerWithSwaggerOperationProcessor
        {
            [HttpGet("/test")]
            public void TestAction() { }
        }

        public class ActionWithSwaggerOperationProcessor
        {
            [SwaggerOperationProcessor(typeof(TestSwaggerOperationProcessor), "Hello from action")]
            [HttpGet("/test")]
            public void TestAction() { }
        }

        private class TestSwaggerOperationProcessor : IOperationProcessor
        {
            public TestSwaggerOperationProcessor(string value)
            {
                Value = value;
            }

            public string Value { get; }

            public bool Process(OperationProcessorContext context)
            {
                context.Document.Info.Title = Value;
                return true;
            }
        }

        [SwaggerResponse(202, typeof(TestModel))]
        private class ControllerWithSwaggerResponseTypeAttribute
        {
            [HttpPut("/test1")]
            public IActionResult UpdateModel(int id) => null;
        }

        private class ActionWithSwaggerResponseAttribute
        {
            [SwaggerResponse(201, typeof(TestModel))]
            [HttpPut("/test1")]
            public IActionResult CreateModel(int id) => null;
        }

        private class ControllerWithParameters
        {
            [HttpPost(nameof(FromHeaderParameter))]
            public IActionResult FromHeaderParameter([FromHeader] string headerParameter) => null;

            [HttpPost(nameof(FromBodyParameter))]
            public IActionResult FromBodyParameter([FromBody] TestModel model) => null;

            [HttpPost(nameof(FromFormParameter))]
            public IActionResult FromFormParameter([FromForm] string formParameter) => null;

            [HttpPost(nameof(FileParameter))]
            public IActionResult FileParameter(IFormFileCollection formFiles) => null;

            [HttpPost(nameof(ComplexFromQueryParameter))]
            public IActionResult ComplexFromQueryParameter(ComplexModel model) => null;

            [HttpPost(nameof(QueryParameter))]
            public IActionResult QueryParameter(string queryParameter1, string queryParameter2 = null) => null;
        }

        private class ControllerWithBoundProperties
        {
            [FromRoute]
            public int Id { get; set; }

            [FromHeader]
            public string HeaderValue { get; set; }

            [HttpPost("Action")]
            public IActionResult TestAction([FromBody] TestModel model) => null;
        }

        public class ControllerWithReCodeAttribute
        {
            [HttpGet("Action")]
            [ReDocCodeSample("foo", "bar")]
            [ReDocCodeSample("baz", "buz")]
            public void Run()
            {
            }
        }

        private class TestModel { }

        private class ComplexModel
        {
            [FromRoute]
            public int Id { get; set; }

            [FromBody]
            public string Header { get; set; }

            [FromQuery(Name = "q")]
            public int[] QueryValues { get; set; }
        }

        private class CustomBindingSourceAttribute : Attribute, IBindingSourceMetadata
        {
            public BindingSource BindingSource => BindingSource.ModelBinding;
        }

        public class ReDocCodeSampleAttribute : SwaggerOperationProcessorAttribute
        {
            public ReDocCodeSampleAttribute(string language, string source)
                : base(typeof(ReDocCodeSampleAppender), language, source)
            {
            }

            internal class ReDocCodeSampleAppender : IOperationProcessor
            {
                private readonly string _language;
                private readonly string _source;
                private const string ExtensionKey = "x-code-samples";

                public ReDocCodeSampleAppender(string language, string source)
                {
                    _language = language;
                    _source = source;
                }

                public bool Process(OperationProcessorContext context)
                {
                    if (context.OperationDescription.Operation.ExtensionData == null)
                    {
                        context.OperationDescription.Operation.ExtensionData = new Dictionary<string, object>();
                    }

                    var data = context.OperationDescription.Operation.ExtensionData;
                    if (!data.ContainsKey(ExtensionKey))
                    {
                        data[ExtensionKey] = new List<ReDocCodeSample>();
                    }

                    var samples = (List<ReDocCodeSample>)data[ExtensionKey];
                    samples.Add(new ReDocCodeSample
                    {
                        Language = _language,
                        Source = _source,
                    });

                    return true;
                }
            }

            public class ReDocCodeSample
            {
                public string Language { get; set; }

                public string Source { get; set; }
            }
        }

        #endregion

        #region ApiDescription generation

        private static ApiDescriptionGroupCollection GetApiDescriptionGroups(Type controllerType)
        {
            var options = Mock.Of<IOptions<MvcOptions>>(m => m.Value == new MvcOptions());
            var applicationPartManager = new ApplicationPartManager();
            applicationPartManager.FeatureProviders.Add(new TestControllerFeatureProvider(controllerType));

            var actionDescriptorProvider = new ControllerActionDescriptorProvider(
                applicationPartManager,
                new IApplicationModelProvider[]
                {
                    new DefaultApplicationModelProvider(options, new EmptyModelMetadataProvider()),
                    new MakeApiExplorerVisibleProvider(),
                },
                options);

            var actionDescriptorProviderContext = new ActionDescriptorProviderContext();
            actionDescriptorProvider.OnProvidersExecuting(actionDescriptorProviderContext);

            var apiDescriptionProvider = new DefaultApiDescriptionProvider(options, Mock.Of<IInlineConstraintResolver>(),
                new EmptyModelMetadataProvider(), new ActionResultTypeMapper());

            var apiDescriptionProviderContext = new ApiDescriptionProviderContext(actionDescriptorProviderContext.Results.ToArray());
            apiDescriptionProvider.OnProvidersExecuting(apiDescriptionProviderContext);

            var groups = apiDescriptionProviderContext.Results.GroupBy(a => a.GroupName)
                .Select(g => new ApiDescriptionGroup(g.Key, g.ToArray()))
                .ToArray();

            return new ApiDescriptionGroupCollection(groups, version: 1);
        }

        private static ApiDescriptionGroupCollection Get2_1_ApiDescriptionGroups(Type controllerType)
        {
            var apiDescriptionGroup = GetApiDescriptionGroups(controllerType);
            foreach (var apiDescription in apiDescriptionGroup.Items.SelectMany(c => c.Items))
            {
                apiDescription.SupportedResponseTypes.Add(new ApiResponseType2_1
                {
                    StatusCode = 0,
                    Type = typeof(ProblemDetails),
                    IsDefaultResponse = true,
                });
            }

            return apiDescriptionGroup;
        }

        private class TestControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
        {
            private readonly Type _controllerType;

            public TestControllerFeatureProvider(Type controllerType)
            {
                _controllerType = controllerType;
            }

            public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
            {
                feature.Controllers.Add(_controllerType.GetTypeInfo());
            }
        }

        private class MakeApiExplorerVisibleProvider : IApplicationModelProvider
        {
            public int Order => 0;

            public void OnProvidersExecuted(ApplicationModelProviderContext context)
            {
            }

            public void OnProvidersExecuting(ApplicationModelProviderContext context)
            {
                context.Result.ApiExplorer.IsVisible = true;
            }
        }

        private class ProblemDetails { }

        private class ApiResponseType2_1 : ApiResponseType
        {
#pragma warning disable 108,114
            public bool IsDefaultResponse { get; set; }
#pragma warning restore 108,114
        }
        #endregion
    }
}
