using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.Generation.TypeMappers;
using NSwag.Annotations;

namespace NSwag.Generation.WebApi.Tests.Attributes
{
    [TestClass]
    public class ComplexParametersTests
    {
        public class MyParameter
        {
            public string Foo { get; set; }

            /// <summary>My comment.</summary>
            [JsonProperty("bar")]
            public string Bar { get; set; }
        }

        public class MyMappedParameter
        {
            public string Foo { get; set; }

            /// <summary>My comment.</summary>
            [JsonProperty("bar")]
            public string Bar { get; set; }
        }

        /// <summary>
        /// Since this class has a [WillReadBody(false)] attribute, OperationParameterProcessor should treat it as a query param
        /// </summary>
        [NSwag.Annotations.WillReadBody(false)]
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
        public sealed class CustomFromUriParameterBinderAttribute : ParameterBindingAttribute
        {
            public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
            {
                if (parameter == null)
                {
                    throw new Exception("parameter");
                }

                return new CustomParameterBinding(parameter);
            }
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
        public class WillReadBodyAttribute : Attribute
        {
        }

        /// <summary>
        /// Since this class has a [WillReadBody] attribute with no WillReadBody property,
        /// OperationParameterProcessor should treat it as a body param.
        /// </summary>
        [ComplexParametersTests.WillReadBody]
        public sealed class CustomFromBodyParameterBinderAttribute : ParameterBindingAttribute
        {
            public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
            {
                if (parameter == null)
                {
                    throw new Exception("parameter");
                }

                if (parameter.Configuration.DependencyResolver == null)
                {
                    return null;
                }

                return new CustomParameterBinding(parameter);
            }
        }

        /// <summary>
        /// Since this class has no [WillReadBody], OperationParameterProcessor should treat it as a body param
        /// </summary>
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
        public sealed class CustomFromBody2ParameterBinderAttribute : ParameterBindingAttribute
        {
            public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
            {
                if (parameter == null)
                {
                    throw new Exception("parameter");
                }

                return new CustomParameterBinding(parameter);
            }
        }

        public class CustomParameterBinding : HttpParameterBinding
        {
            private static readonly Task<object> _CompletedTaskReturningNull = Task.FromResult<object>(null);

            public CustomParameterBinding(HttpParameterDescriptor parameter) : base(parameter)
            {
            }

            public override Task ExecuteBindingAsync(
                ModelMetadataProvider metadataProvider,
                HttpActionContext actionContext,
                CancellationToken cancellationToken)
            {
                actionContext.ActionArguments[Descriptor.ParameterName] = new MyParameter { Bar = "hello", Foo = "world" };

                return _CompletedTaskReturningNull;
            }
        }

        public class TestController : ApiController
        {
            public string WithoutAttribute(MyParameter data)
            {
                return string.Empty;
            }

            /// <summary>My comment.</summary>
            /// <remarks>My remarks.</remarks>
            public string WithFromUriAttribute([FromUri] MyParameter data)
            {
                return string.Empty;
            }

            public string WithFromBodyAttribute([FromBody] MyParameter data)
            {
                return string.Empty;
            }

            public string WithCustomFromUriParameterBinder([CustomFromUriParameterBinder]MyParameter data)
            {
                return string.Empty;
            }

            public string WithCustomFromUriTypeMappedParameter([CustomFromUriParameterBinder]MyMappedParameter mappedParameter = null)
            {
                return string.Empty;
            }

            public string WithCustomFromBodyParameterBinder([CustomFromBodyParameterBinder]MyParameter data)
            {
                return string.Empty;
            }

            public string WithCustomFromBody2ParameterBinder([CustomFromBody2ParameterBinder]MyParameter data)
            {
                return string.Empty;
            }
        }

        [TestMethod]
        public async Task When_parameter_is_complex_then_it_is_a_body_parameter()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings
            {
                DefaultUrlTemplate = "api/{controller}/{action}/{id}"
            });

            // Act
            var document = await generator.GenerateForControllerAsync<TestController>();
            var operation = document.Operations.Single(o => o.Operation.OperationId == "Test_WithoutAttribute").Operation;

            // Assert
            Assert.AreEqual(OpenApiParameterKind.Body, operation.ActualParameters[0].Kind);
            Assert.AreEqual("data", operation.ActualParameters[0].Name);
        }

        [TestMethod]
        public async Task When_parameter_is_complex_and_has_FromUri_then_complex_object_properties_are_added()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings
            {
                DefaultUrlTemplate = "api/{controller}/{action}/{id}"
            });

            // Act
            var document = await generator.GenerateForControllerAsync<TestController>();
            var operation = document.Operations.Single(o => o.Operation.OperationId == "Test_WithFromUriAttribute").Operation;

            // Assert
            Assert.AreEqual("My comment.", operation.Summary);
            Assert.AreEqual("My remarks.", operation.Description);
            Assert.AreEqual(OpenApiParameterKind.Query, operation.ActualParameters[0].Kind);
            Assert.AreEqual(OpenApiParameterKind.Query, operation.ActualParameters[1].Kind);
            Assert.AreEqual("Foo", operation.ActualParameters[0].Name);
            Assert.AreEqual("bar", operation.ActualParameters[1].Name);
            Assert.AreEqual("My comment.", operation.ActualParameters[1].Description);
        }

        [TestMethod]
        public async Task When_parameter_is_complex_and_has_FromBody_then_it_is_a_body_parameter()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings
            {
                DefaultUrlTemplate = "api/{controller}/{action}/{id}"
            });

            // Act
            var document = await generator.GenerateForControllerAsync<TestController>();
            var operation = document.Operations.Single(o => o.Operation.OperationId == "Test_WithFromBodyAttribute").Operation;
            var json = document.ToJson();

            // Assert
            Assert.AreEqual(OpenApiParameterKind.Body, operation.ActualParameters[0].Kind);
            Assert.AreEqual("data", operation.ActualParameters[0].Name);
        }

        [TestMethod]
        public async Task When_parameter_is_complex_and_has_CustomBinding_that_will_not_read_body_then_it_is_a_query_parameter()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings
            {
                DefaultUrlTemplate = "api/{controller}/{action}/{id}"
            });

            // Act
            var document = await generator.GenerateForControllerAsync<TestController>();
            var operation = document.Operations.Single(o => o.Operation.OperationId == "Test_WithCustomFromUriParameterBinder").Operation;
            var json = document.ToJson();

            // Assert
            Assert.AreEqual(OpenApiParameterKind.Query, operation.ActualParameters[0].Kind);
            Assert.AreEqual("data", operation.ActualParameters[0].Name);
        }

        [TestMethod]
        public async Task When_parameter_is_complex_and_has_CustomBinding_that_will_not_read_body_with_mapped_parameter_then_it_is_a_query_parameter()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings
            {
                DefaultUrlTemplate = "api/{controller}/{action}/{id}"
            });

            // Act
            generator.Settings.TypeMappers.Add(
                new PrimitiveTypeMapper(
                    typeof(MyMappedParameter),
                    s =>
                    {
                        s.Type = JsonObjectType.String;
                        s.Enumeration.Add("hello");
                        s.Enumeration.Add("world");
                    }));
            var document = await generator.GenerateForControllerAsync<TestController>();
            var operation = document.Operations.Single(o => o.Operation.OperationId == "Test_WithCustomFromUriTypeMappedParameter").Operation;
            var json = document.ToJson();

            // Assert
            Assert.AreEqual(OpenApiParameterKind.Query, operation.ActualParameters[0].Kind);
            Assert.AreEqual(JsonObjectType.String, operation.ActualParameters[0].Type);
            Assert.AreEqual("mappedParameter", operation.ActualParameters[0].Name);
        }

        [TestMethod]
        public async Task When_parameter_is_complex_and_has_CustomBinding_that_will_read_body_then_it_is_a_body_parameter()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings
            {
                DefaultUrlTemplate = "api/{controller}/{action}/{id}"
            });

            // Act
            var document = await generator.GenerateForControllerAsync<TestController>();
            var operation = document.Operations.Single(o => o.Operation.OperationId == "Test_WithCustomFromBodyParameterBinder").Operation;
            var json = document.ToJson();

            // Assert
            Assert.AreEqual(OpenApiParameterKind.Body, operation.ActualParameters[0].Kind);
            Assert.AreEqual("data", operation.ActualParameters[0].Name);
        }

        [TestMethod]
        public async Task When_parameter_is_complex_and_has_CustomBinding_with_no_WillReadBody_then_it_is_a_body_parameter()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings
            {
                DefaultUrlTemplate = "api/{controller}/{action}/{id}"
            });

            // Act
            var document = await generator.GenerateForControllerAsync<TestController>();
            var operation = document.Operations.Single(o => o.Operation.OperationId == "Test_WithCustomFromBody2ParameterBinder").Operation;
            var json = document.ToJson();

            // Assert
            Assert.AreEqual(OpenApiParameterKind.Body, operation.ActualParameters[0].Kind);
            Assert.AreEqual("data", operation.ActualParameters[0].Name);
        }

        public class ComplexPathParameterController : ApiController
        {
            [HttpGet]
            [Route("Filter/{title}")]
            public string Filter([FromUri]ComplexGetRequest model)
            {
                return "foobar = " + model.Title + " => " + model.Bar;
            }

            public class ComplexGetRequest
            {
                [System.ComponentModel.DataAnnotations.Required]
                [FromRoute]
                public string Title { get; set; }

                public string Bar { get; set; }
            }
        }

        [TestMethod]
        public async Task When_complex_parameter_contains_property_with_FromRoute_attribute_then_it_is_generated_as_path_parameter()
        {
            // FromRouteAttribute is only available in ASP.NET Core
            // Issue: https://github.com/RicoSuter/NSwag/issues/513

            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings
            {
                IsAspNetCore = true,
                DefaultPropertyNameHandling = PropertyNameHandling.CamelCase
            });

            // Act
            var document = await generator.GenerateForControllerAsync<ComplexPathParameterController>();
            var operation = document.Operations.First().Operation;
            var json = document.ToJson();

            // Assert
            Assert.AreEqual(OpenApiParameterKind.Path, operation.Parameters[0].Kind);
            Assert.AreEqual("title", operation.Parameters[0].Name);
            Assert.AreEqual(OpenApiParameterKind.Query, operation.Parameters[1].Kind);
            Assert.AreEqual("bar", operation.Parameters[1].Name);
        }
    }

    public class FromRouteAttribute : Attribute
    {
        public string Name { get; set; }
    }
}