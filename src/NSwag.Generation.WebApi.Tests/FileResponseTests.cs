using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NJsonSchema;
using NSwag.Generation.WebApi;

namespace NSwag.Generation.WebApi.Tests
{
    [TestClass]
    public class FileResponseTests
    {
        public class FileResponseController : ApiController
        {
            public IHttpActionResult GetFile(string fileName)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public async Task When_response_is_file_then_mime_type_is_bytes()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            
            // Act
            var document = await generator.GenerateForControllerAsync<FileResponseController>();
            var json = document.ToJson();

            // Assert
            var operation = document.Operations.First().Operation;

            //Assert.AreEqual("application/octet-stream", operation.ActualProduces.First());
            Assert.AreEqual(JsonObjectType.File, operation.ActualResponses.First().Value.Schema.Type);
            
            // TODO: File response should produce application/octet-stream
        }
    }
}