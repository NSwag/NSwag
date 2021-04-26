using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NJsonSchema;
using NSwag.Generation.WebApi;

namespace NSwag.Generation.WebApi.Tests.Attributes
{
    [TestClass]
    public class PrimitiveQueryParametersTests
    {
        public class TestController : ApiController
        {
            public string WithoutAttribute(string foo)
            {
                return String.Empty;
            }

            public string WithFromUriAttribute([FromUri] string foo)
            {
                return String.Empty;
            }

            public string WithFromBodyAttribute([FromBody] string foo)
            {
                return String.Empty;
            }

            public class FilterOptions
            {
                [JsonProperty("currentStates")]
                public string[] CurrentStates { get; set; }
            }

            public void Filter([FromUri] FilterOptions filter)
            {

            }
        }

        [TestMethod]
        public async Task When_parameter_is_primitive_then_it_is_a_query_parameter()
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
            Assert.AreEqual(OpenApiParameterKind.Query, operation.ActualParameters[0].Kind);
        }

        [TestMethod]
        public async Task When_parameter_is_primitive_and_has_FromUri_then_it_is_a_query_parameter()
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
            Assert.AreEqual(OpenApiParameterKind.Query, operation.ActualParameters[0].Kind);
        }


        [TestMethod]
        public async Task When_parameter_is_primitive_and_has_FromBody_then_it_is_a_body_parameter()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings
            {
                DefaultUrlTemplate = "api/{controller}/{action}/{id}"
            });

            // Act
            var document = await generator.GenerateForControllerAsync<TestController>();
            var operation = document.Operations.Single(o => o.Operation.OperationId == "Test_WithFromBodyAttribute").Operation;

            // Assert
            Assert.AreEqual(OpenApiParameterKind.Body, operation.ActualParameters[0].Kind);
        }

        [TestMethod]
        public async Task When_FromUri_has_array_property_then_collectionFormat_is_set_to_multi()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings
            {
                DefaultUrlTemplate = "api/{controller}/{action}/{id}"
            });

            // Act
            var document = await generator.GenerateForControllerAsync<TestController>();
            var operation = document.Operations.Single(o => o.Operation.OperationId == "Test_Filter").Operation;

            // Assert
            Assert.AreEqual(OpenApiParameterCollectionFormat.Multi, operation.ActualParameters[0].CollectionFormat);
        }

        public class ControllerWithArrayQueryParameter : ApiController
        {
            public string Foo([FromUri] string[] ids)
            {
                return string.Empty;
            }
        }

        [TestMethod]
        public async Task When_parameter_is_array_and_has_FromUri_then_it_is_a_query_parameter()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings
            {
                DefaultUrlTemplate = "api/{controller}/{action}/{id}"
            });

            // Act
            var document = await generator.GenerateForControllerAsync<ControllerWithArrayQueryParameter>();
            var json = document.ToJson();

            // Assert
            var operation = document.Operations.First().Operation;
            var parameter = operation.ActualParameters.First();

            Assert.AreEqual(OpenApiParameterKind.Query, parameter.Kind);
            Assert.AreEqual(JsonObjectType.Array, parameter.Type);
            Assert.AreEqual(OpenApiParameterCollectionFormat.Multi, parameter.CollectionFormat);
        }

        public class MyTestClass : ApiController
        {
            [Route("Foo")]
            public Task Foo(int id, bool? someNullableParam)
            {
                return Task.CompletedTask;
            }

            [Route("Bar")]
            public Task Bar(int id, bool? someNullableParam = null)
            {
                return Task.CompletedTask;
            }
        }

        [TestMethod]
        public async Task When_parameter_is_nullable_then_it_is_optional()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings { IsAspNetCore = true });

            // Act
            var document = await generator.GenerateForControllerAsync<MyTestClass>();
            var json = document.ToJson();

            // Assert
            Assert.IsTrue(document.Operations.First().Operation.Parameters[1].IsRequired);
            Assert.IsFalse(document.Operations.Last().Operation.Parameters[1].IsRequired);
        }
    }
}
