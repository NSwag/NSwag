using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NSwag.Generation.WebApi.Tests.Attributes
{
    [TestClass]
    public class AcceptVerbsTests
    {
        public class TestController : ApiController
        {
            [AcceptVerbs("Post")]
            public int AddPost(int a, int b)
            {
                return a + b; 
            }

            [AcceptVerbs("Get")]
            public int AddGet(int a, int b)
            {
                return a + b;
            }

            [AcceptVerbs("Delete")]
            public int AddDelete(int a, int b)
            {
                return a + b;
            }

            [AcceptVerbs("Put")]
            public int AddPut(int a, int b)
            {
                return a + b;
            }
        }

        [TestMethod]
        public async Task When_accept_verbs_attribute_with_post_is_used_then_http_method_is_correct()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync<TestController>();
            var operation = document.Operations.First(o => o.Operation.OperationId == "Test_AddPost");

            // Assert
            Assert.AreEqual(OpenApiOperationMethod.Post, operation.Method);
        }

        [TestMethod]
        public async Task When_accept_verbs_attribute_with_get_is_used_then_http_method_is_correct()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync<TestController>();
            var operation = document.Operations.First(o => o.Operation.OperationId == "Test_AddGet");

            // Assert
            Assert.AreEqual(OpenApiOperationMethod.Get, operation.Method);
        }

        [TestMethod]
        public async Task When_accept_verbs_attribute_with_delete_is_used_then_http_method_is_correct()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync<TestController>();
            var operation = document.Operations.First(o => o.Operation.OperationId == "Test_AddDelete");

            // Assert
            Assert.AreEqual(OpenApiOperationMethod.Delete, operation.Method);
        }

        [TestMethod]
        public async Task When_accept_verbs_attribute_with_put_is_used_then_http_method_is_correct()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());

            // Act
            var document = await generator.GenerateForControllerAsync<TestController>();
            var operation = document.Operations.First(o => o.Operation.OperationId == "Test_AddPut");

            // Assert
            Assert.AreEqual(OpenApiOperationMethod.Put, operation.Method);
        }
    }
}