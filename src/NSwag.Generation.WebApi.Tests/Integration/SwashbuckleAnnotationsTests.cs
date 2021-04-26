using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.Generation.TypeMappers;

namespace NSwag.Generation.WebApi.Tests.Integration
{
    [TestClass]
    public class SwashbuckleAnnotationsTests
    {
        // https://github.com/RicoSuter/NSwag/issues/494

        public class JsonDate
        {

        }

        public class LockedFlight
        {
            public string Name { get; set; }

            public JsonDate Date { get; set; }
        }

        [Swashbuckle.Swagger.Annotations.SwaggerResponse(HttpStatusCode.Forbidden, "The user is not authorized")]
        public class MyController : ApiController
        {
            [HttpPost]

            [NSwag.Annotations.SwaggerResponse(200, typeof(LockedFlight), Description = "OK")]
            [NSwag.Annotations.SwaggerResponse(201, typeof(LockedFlight), Description = "Created")]
            [NSwag.Annotations.SwaggerOperation("AddFlightLock")]

            [Swashbuckle.Swagger.Annotations.SwaggerResponse(200, "OK", typeof(LockedFlight))]
            [Swashbuckle.Swagger.Annotations.SwaggerResponse(201, "Created", typeof(LockedFlight))]
            [Swashbuckle.Swagger.Annotations.SwaggerOperation("AddFlightLock")]

            [System.Web.Http.Description.ResponseType(typeof(LockedFlight))]
            public Task<IHttpActionResult> Post(string id)
            {
                return Task.FromResult<IHttpActionResult>(null);
            }
        }

        [TestMethod]
        public async Task When_redundant_attributes_are_available_then_output_is_correct()
        {
            // Arrange
            var settings = new WebApiOpenApiDocumentGeneratorSettings
            {
                TypeMappers =
                {
                    new ObjectTypeMapper(typeof(JsonDate), new JsonSchema
                    {
                        Type = JsonObjectType.String,
                        Format = "date"
                    })
                },
                AddMissingPathParameters = false,
                SchemaType = SchemaType.Swagger2,
                GenerateKnownTypes = true,
                FlattenInheritanceHierarchy = true,
                DefaultEnumHandling = EnumHandling.String,
                IsAspNetCore = false,
            };
            var generator = new WebApiOpenApiDocumentGenerator(settings);

            // Act
            var document = await generator.GenerateForControllerAsync<MyController>();
            var json = document.ToJson();

            // Assert
            Assert.IsTrue(json.Contains("\"$ref\": \"#/definitions/LockedFlight\""));
        }

        [TestMethod]
        public async Task When_SwaggerResponseAttribute_is_on_class_then_it_is_applied_to_all_methods()
        {
            // Arrange
            var settings = new WebApiOpenApiDocumentGeneratorSettings();
            var generator = new WebApiOpenApiDocumentGenerator(settings);

            // Act
            var document = await generator.GenerateForControllerAsync<MyController>();
            var json = document.ToJson();

            // Assert
            Assert.IsTrue(document.Operations.First().Operation.ActualResponses.ContainsKey("403"));
        }
    }
}
