using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NJsonSchema;
using NSwag.Annotations;
using NSwag.Generation.WebApi;

namespace NSwag.Generation.WebApi.Tests.Attributes
{
    [TestClass]
    public class ResponseAttributeTests
    {
        public class ResponseAttributeTestController : ApiController
        {
            [Route("Foo")]
            [ResponseType(HttpStatusCode.Conflict, typeof(string))]
            public void Foo()
            {

            }

            [Route("Bar")]
            [SwaggerResponse(HttpStatusCode.Created, typeof(int))]
            public void Bar()
            {

            }
        }

        [TestMethod]
        public async Task When_operation_has_ResponseTypeAttribute_then_it_is_processed()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync<ResponseAttributeTestController>();

            // Assert
            var fooOperation = document.Operations.Single(o => o.Operation.OperationId == "ResponseAttributeTest_Foo");
            Assert.AreEqual("409", fooOperation.Operation.ActualResponses.First().Key);
            Assert.AreEqual(JsonObjectType.String, fooOperation.Operation.ActualResponses.First().Value.Schema.Type);
        }

        [TestMethod]
        public async Task When_operation_has_SwaggerResponseAttribute_then_it_is_processed()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync<ResponseAttributeTestController>();

            // Assert
            var barOperation = document.Operations.Single(o => o.Operation.OperationId == "ResponseAttributeTest_Bar");
            Assert.AreEqual("201", barOperation.Operation.ActualResponses.First().Key);
            Assert.AreEqual(JsonObjectType.Integer, barOperation.Operation.ActualResponses.First().Value.Schema.Type);
        }

        public class MultipleAttributesController : ApiController
        {
            /// <returns>Bar.</returns>
            [Route("foo")]
            [SwaggerResponse(HttpStatusCode.OK, typeof(void), Description = "Foo")]
            public void Foo()
            {

            }

            /// <returns>Bar.</returns>
            [Route("bar")]
            public void Bar()
            {

            }

            [ProducesResponseType(Type = typeof(void))]
            [SwaggerResponse(HttpStatusCode.OK, typeof(void), Description = "Bar")]
            public void Baz()
            {

            }
        }

        public class ProducesResponseTypeAttribute : Attribute { public Type Type { get; set; } public int StatusCode { get; set;} }

        [TestMethod]
        public async Task When_response_description_is_defined_then_xml_summary_is_ignored()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync<MultipleAttributesController>();

            // Assert
            Assert.IsFalse(document.Operations.ElementAt(0).Operation.ActualResponses.First().Value.Description.Contains("Bar"));
            Assert.IsTrue(document.Operations.ElementAt(1).Operation.ActualResponses.First().Value.Description.Contains("Bar"));
            Assert.IsFalse(document.Operations.ElementAt(2).Operation.ActualResponses.First().Value.Description.Contains("or"));
        }
    }
}
