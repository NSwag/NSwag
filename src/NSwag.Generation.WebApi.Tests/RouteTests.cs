using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSwag.Annotations;

namespace NSwag.Generation.WebApi.Tests
{
    [TestClass]
    public class RouteTests
    {
        // From https://github.com/RicoSuter/NSwag/issues/664

        public class ProductPagedResult
        {
            public string Foo { get; set; }
        }

        public class Product
        {
            public string Bar { get; set; }
        }

        public class AddProductPayload
        {
            public string Baz { get; set; }
        }

        public class ProductsController : ApiController
        {
            [SwaggerOperation("GetProducts")]
            [HttpGet, Route("products")]
            public Task<IHttpActionResult> GetProducts([FromUri] ProductPagedResult payload)
            {
                return Task.FromResult<IHttpActionResult>(null);
            }

            [SwaggerOperation("GetProductByUserDefinedId")]
            [HttpGet, Route("products/{userDefinedId}")]
            public IHttpActionResult GetProduct(string userDefinedId)
            {
                return null;
            }

            [SwaggerOperation("GetProductByUniqueId")]
            [HttpGet, Route("products/{id:guid}")]
            public IHttpActionResult GetProductByUniqueId(Guid id)
            {
                return null;
            }

            [SwaggerOperation("DeleteProductByUserDefinedId")]
            [HttpDelete, Route("products/{userDefinedId}")]
            public Task<IHttpActionResult> Delete(string userDefinedId)
            {
                return Task.FromResult<IHttpActionResult>(null);
            }

            [SwaggerOperation("DeleteProductByUniqueId")]
            [HttpDelete, Route("products/{id:guid}")]
            public Task<IHttpActionResult> DeleteByUniqueId()
            {
                return Task.FromResult<IHttpActionResult>(null);
            }

            [SwaggerOperation("PutProductByUserDefinedId")]
            [HttpPut, Route("products/{userDefinedId}")]
            public Task<IHttpActionResult> Put(string userDefinedId, [FromBody] Product data)
            {
                return Task.FromResult<IHttpActionResult>(null);
            }

            [SwaggerOperation("PutProductByUniqueId")]
            [HttpPut, Route("products/{id:guid}")]
            public Task<IHttpActionResult> Put(Guid id, [FromBody] Product data)
            {
                return Task.FromResult<IHttpActionResult>(null);
            }

            [SwaggerOperation("PostProducts")]
            [HttpPost, Route("products")]
            public Task<IHttpActionResult> FetchAll([FromBody] AddProductPayload data)
            {
                return Task.FromResult<IHttpActionResult>(null);
            }
        }

        [TestMethod]
        public async Task When_swagger_spec_is_generated_then_no_route_problem_is_detected()
        {
            // Arrange
            var settings = new WebApiOpenApiDocumentGeneratorSettings
            {
                DefaultUrlTemplate = "{controller}/{id}",
                AddMissingPathParameters = false,
            };

            // Act
            var generator = new WebApiOpenApiDocumentGenerator(settings);
            var document = await generator.GenerateForControllerAsync<ProductsController>();
            var swaggerSpecification = document.ToJson();

            // Assert
            Assert.IsNotNull(swaggerSpecification);
        }

        public class WildcardPathController : ApiController
        {
            [Route("path/{*param}")]
            public void Foo(string param)
            {

            }
        }

        [TestMethod]
        public async Task When_path_parameter_has_wildcard_then_it_is_in_path()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync<WildcardPathController>();

            // Assert
            var operation = document.Operations.First();
            var parameter = operation.Operation.Parameters.First();

            Assert.AreEqual("/path/{param}", operation.Path);
            Assert.AreEqual("param", parameter.Name);
            Assert.AreEqual(OpenApiParameterKind.Path, parameter.Kind);
        }
    }
}
