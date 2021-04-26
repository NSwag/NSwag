using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NJsonSchema;
using NJsonSchema.Annotations;

namespace NSwag.Generation.WebApi.Tests
{
    [TestClass]
    public class ParameterTests
    {
        public interface IFormFile
        {
        }

        public interface IActionResult
        {
        }

        public class FromUriParameterController : ApiController
        {
            public class ComplexClass
            {
                public string Foo { get; set; }

                public string Bar { get; set; }
            }

            public class ComplexClassWithEmbeddedPathParam
            {
                public Guid Id { get; set; }
            }

            [HttpPost, Route("upload")]
            public IActionResult Upload([FromUri]ComplexClass parameters)
            {
                throw new NotImplementedException();
            }

            [HttpGet, Route("fetch/{id}")]
            public IHttpActionResult Fetch([FromUri]ComplexClassWithEmbeddedPathParam model)
            {
                return Ok(string.Empty);
            }

            [HttpGet, Route("fetch-all")]
            public IHttpActionResult FetchAll([FromUri] ComplexClass model)
            {
                return Ok(string.Empty);
            }
        }

        [TestMethod]
        public async Task When_parameter_is_from_uri_then_two_params_are_generated()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync(typeof(FromUriParameterController));

            // Assert
            var operation = document.Paths["/upload"][OpenApiOperationMethod.Post];

            Assert.AreEqual(JsonObjectType.String, operation.ActualParameters.Single(p => p.Name == "Foo").Type);
            Assert.AreEqual(JsonObjectType.String, operation.ActualParameters.Single(p => p.Name == "Bar").Type);

            Assert.IsTrue(operation.ActualParameters.Any(p => p.Name == "Foo"));
            Assert.IsTrue(operation.ActualParameters.Any(p => p.Name == "Bar"));

            Assert.IsNull(operation.Consumes);
        }

        [TestMethod]
        public async Task When_path_parameter_embedded_in_ComplexType_from_uri_then_path_param_is_generated()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync(typeof(FromUriParameterController));

            // Assert
            var operation = document.Paths["/fetch/{id}"][OpenApiOperationMethod.Get];
            var parameter = operation.ActualParameters.Single(p => p.Name == "Id");

            Assert.AreEqual(JsonObjectType.String, parameter.Type);
            Assert.AreEqual(OpenApiParameterKind.Path, parameter.Kind);

            Assert.IsNull(operation.Consumes);
        }

        [TestMethod]
        public async Task When_parameters_are_from_uri_then_query_params_are_generated()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync(typeof(FromUriParameterController));

            // Assert
            var operation = document.Paths["/fetch-all"][OpenApiOperationMethod.Get];

            Assert.AreEqual(JsonObjectType.String, operation.ActualParameters.Single(p => p.Name == "Foo").Type);
            Assert.AreEqual(OpenApiParameterKind.Query, operation.ActualParameters.Single(p => p.Name == "Foo").Kind);

            Assert.AreEqual(JsonObjectType.String, operation.ActualParameters.Single(p => p.Name == "Bar").Type);
            Assert.AreEqual(OpenApiParameterKind.Query, operation.ActualParameters.Single(p => p.Name == "Bar").Kind);

            Assert.IsNull(operation.Consumes);
        }

        public class ConstrainedRoutePathController : ApiController
        {
            [Route("{id:long:min(1)}")]
            public object Get(long id)
            {
                return null;
            }
        }

        [TestMethod]
        public async Task When_web_api_path_has_constraints_then_they_are_removed_in_the_swagger_spec()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync(typeof(ConstrainedRoutePathController));

            // Assert
            var path = document.Paths.First().Key;

            Assert.AreEqual("/{id}", path);
        }

        [Route("account/{action}/{id?}")]
        public class AccountController : ApiController
        {
            [HttpGet]
            public string Get()
            {
                return null;
            }

            [HttpGet]
            public string GetAll()
            {
                return null;
            }

            [HttpPost]
            public Task<IHttpActionResult> Post([FromBody] object model)
            {
                return Task.FromResult<IHttpActionResult>(null);
            }

            [HttpPost]
            public Task<IHttpActionResult> Verify([FromBody] object model)
            {
                return Task.FromResult<IHttpActionResult>(null);
            }

            [HttpPost]
            public Task<IHttpActionResult> Confirm([FromBody] object model)
            {
                return Task.FromResult<IHttpActionResult>(null);
            }
        }

        [TestMethod]
        public async Task When_class_has_RouteAttribute_with_placeholders_then_they_are_correctly_replaced()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync(typeof(AccountController));

            // Assert
            Assert.IsTrue(document.Paths.ContainsKey("/account/Get"));
            Assert.IsTrue(document.Paths.ContainsKey("/account/GetAll"));
            Assert.IsTrue(document.Paths.ContainsKey("/account/Post"));
            Assert.IsTrue(document.Paths.ContainsKey("/account/Verify"));
            Assert.IsTrue(document.Paths.ContainsKey("/account/Confirm"));
        }

        [RoutePrefix("api/{id1}")]
        public class MyApiController : ApiController
        {
            [HttpGet]
            [Route("Services")]
            public IHttpActionResult Operation1([FromUri(Name = "id1")] string id1)
            {
                return null;
            }

            [HttpGet]
            [Route("Services/{id2}")]
            public IHttpActionResult Operation1([FromUri(Name = "id1")] string id1, [FromUri(Name = "id2")] string x)
            {
                return null;
            }
        }

        [TestMethod]
        public async Task When_FromUri_has_name_then_parameter_name_is_correct()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync(typeof(MyApiController));
            var json = document.ToJson();

            // Assert
            Assert.IsTrue(document.Paths.ContainsKey("/api/{id1}/Services"));
            Assert.IsTrue(document.Paths.ContainsKey("/api/{id1}/Services/{id2}"));
        }

        [RoutePrefix("api/{id1}")]
        public class MyControllerWithJsonSchemaTypeParameter : ApiController
        {
            [HttpGet]
            [Route("Services")]
            public IHttpActionResult Operation1([JsonSchemaType(typeof(int))]string id1)
            {
                return null;
            }
        }

        [TestMethod]
        public async Task When_parameter_has_JsonSchemaTypeAttribute_then_it_is_processed()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync(typeof(MyControllerWithJsonSchemaTypeParameter));
            var json = document.ToJson();

            // Assert
            var parameter = document.Operations.First().Operation.ActualParameters.First().ActualParameter;
            Assert.IsTrue(parameter.ActualTypeSchema.Type.HasFlag(JsonObjectType.Integer));
        }
    }
}
