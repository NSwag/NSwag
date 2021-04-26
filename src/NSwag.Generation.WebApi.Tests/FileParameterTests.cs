using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NJsonSchema;

namespace NSwag.Generation.WebApi.Tests
{
    [TestClass]
    public class FileParameterTests
    {
        public interface IFormFile
        {
        }

        public interface IActionResult
        {
        }

        public class FromUriFileParameterController : ApiController
        {
            public class ComplexClass
            {
                [JsonProperty("formFile")]
                public IFormFile FormFile { get; set; }

                public string CustomLocationToSave { get; set; }
            }

            [HttpPost, Route("upload")]
            public IActionResult Upload([FromUri] ComplexClass parameters)
            {
                // TODO: Check if this is the correct behavior or if FromUri can be omitted when a property is a file
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public async Task When_parameter_is_from_uri_and_has_file_then_two_params_and_consumes_is_correct()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync(typeof(FromUriFileParameterController));
            var json = document.ToJson();

            // Assert
            var operation = document.Paths["/upload"][OpenApiOperationMethod.Post];

            Assert.AreEqual(JsonObjectType.File, operation.ActualParameters.Single(p => p.Name == "formFile").Type);
            Assert.IsTrue(operation.ActualParameters.Any(p => p.Name == "formFile"));
            Assert.IsTrue(operation.ActualParameters.Any(p => p.Name == "CustomLocationToSave"));
            Assert.AreEqual("multipart/form-data", operation.Consumes[0]);
        }

        public interface IFormFileCollection
        {
        }

        public class FileCollectionController : ApiController
        {
            [HttpPost, Route("upload")]
            public IActionResult Upload(IFormFileCollection files)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public async Task When_parameter_is_file_collection_then_type_is_correct_and_collection_format_is_multi()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync(typeof(FileCollectionController));

            // Assert
            var operation = document.Paths["/upload"][OpenApiOperationMethod.Post];
            var parameter = operation.ActualParameters.Single(p => p.Name == "files");

            Assert.AreEqual(JsonObjectType.File, parameter.Type);
            Assert.AreEqual(OpenApiParameterCollectionFormat.Multi, parameter.CollectionFormat);

            Assert.AreEqual(1, operation.ActualConsumes.Count());
            Assert.AreEqual("multipart/form-data", operation.ActualConsumes.First());
        }

        public class StreamBodyParameterController
        {
            [HttpPost, Route("upload")]
            public void Upload([FromBody] Stream data)
            {

            }
        }

        [TestMethod]
        public async Task When_body_parameter_is_Stream_then_consumes_is_octet_()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync<StreamBodyParameterController>();
            var json = document.ToJson();

            // Assert
            var operation = document.Paths["/upload"][OpenApiOperationMethod.Post];
            var parameter = operation.ActualParameters.Single(p => p.Name == "data");

            Assert.AreEqual(JsonObjectType.String, parameter.Schema.Type);
            Assert.AreEqual(JsonFormatStrings.Binary, parameter.Schema.Format);

            Assert.AreEqual(2, operation.ActualConsumes.Count());
            Assert.AreEqual("application/octet-stream", operation.ActualConsumes.First());
        }
    }
}
