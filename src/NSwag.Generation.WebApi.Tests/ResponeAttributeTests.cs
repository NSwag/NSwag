using System;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NJsonSchema.Converters;
using NSwag.Annotations;

namespace NSwag.Generation.WebApi.Tests
{
    [TestClass]
    public class ResponeAttributeTests
    {
        [JsonConverter(typeof(JsonInheritanceConverter), "discriminator")]
        [KnownType(typeof(Dog))]
        [KnownType(typeof(Horse))]
        public class Animal
        {
            public string Foo { get; set; }
        }

        public class Dog : Animal
        {
            public string Bar { get; set; }
        }

        public class Horse : Animal
        {
            public string Bar { get; set; }
        }

        public class TestController : ApiController
        {
            [Route("Abc")]
            [SwaggerResponse(HttpStatusCode.OK, typeof(object))]
            [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(Horse))]
            [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(Dog))]
            public object Abc()
            {
                return null;
            }

            [Route("Def")]
            [SwaggerResponse(HttpStatusCode.OK, typeof(object))]
            [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(Animal))]
            [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(Dog))]
            public object Def()
            {
                return null;
            }
        }

        [TestMethod]
        public async Task When_multple_responses_with_same_status_code_are_defined_then_they_are_merged_and_common_base_type_used_as_response_schema()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync<TestController>();

            // Assert
            var operationAbc = document.Operations.Single(o => o.Path.Contains("Abc"));
            var responseAbc = operationAbc.Operation.ActualResponses.First(r => r.Key == "500").Value;

            Assert.AreEqual(document.Definitions["Animal"].ActualSchema, responseAbc.Schema.ActualSchema);
            Assert.AreEqual(2, responseAbc.ExpectedSchemas.Count);

            var operationDef = document.Operations.Single(o => o.Path.Contains("Abc"));
            var responseDef = operationDef.Operation.ActualResponses.First(r => r.Key == "500").Value;

            Assert.AreEqual(document.Definitions["Animal"].ActualSchema, responseDef.Schema.ActualSchema);
            Assert.AreEqual(2, responseDef.ExpectedSchemas.Count);
        }

        [Authorize]
        public class MyApiController : ApiController
        {
            [Route("{username}/cookies")]
            [HttpGet]
            [SwaggerResponse(HttpStatusCode.OK, typeof(CookieCollection))]
            public Task<IHttpActionResult> GetCookies(string username)
            {
                return Task.FromResult<IHttpActionResult>(null);
            }
        }

        [TestMethod]
        public async Task When_spec_is_generated_then_no_NPE_is_thrown()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync<MyApiController>();
            var json = document.ToJson();

            // Assert
            Assert.IsTrue(document.Operations.First().Operation.ActualResponses.First().Value.Schema.Item.IsAnyType);
        }
    }
}
