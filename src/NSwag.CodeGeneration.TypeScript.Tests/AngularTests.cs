using System.Threading.Tasks;
using Xunit;
using NSwag.Generation.WebApi;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace NSwag.CodeGeneration.TypeScript.Tests
{
    public class AngularTests
    {
        public class Foo
        {
            public string Bar { get; set; }
        }

        [Route("[controller]/[action]")]
        public class DiscussionController : Controller
        {
            [HttpPost]
            public void AddMessage([FromBody, Required]Foo message)
            {
            }

            [HttpPost]
            public void GenericRequestTest1(GenericRequest1 request)
            {

            }

            [HttpPost]
            public void GenericRequestTest2(GenericRequest2 request)
            {

            }
        }

        public class GenericRequestBase<T>
            where T : RequestBodyBase
        {
            [Required]
            public T Request { get; set; }
        }

        public class RequestBodyBase
        {

        }

        public class RequestBody : RequestBodyBase
        {

        }

        public class GenericRequest1 : GenericRequestBase<RequestBodyBase>
        {

        }

        public class GenericRequest2 : GenericRequestBase<RequestBody>
        {

        }

        public class UrlEncodedRequestConsumingController : Controller
        {
            [HttpPost]
            [Consumes("application/x-www-form-urlencoded")]
            public void AddMessage([FromForm]Foo message, [FromForm]string messageId)
            {
            }
        }

        [Fact]
        public async Task When_return_value_is_void_then_client_returns_observable_of_void()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await generator.GenerateForControllerAsync<DiscussionController>();
            var json = document.ToJson();

            // Act
            var codeGen = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                Template = TypeScriptTemplate.Angular,
                GenerateClientInterfaces = true,
                TypeScriptGeneratorSettings =
                {
                    TypeScriptVersion = 2.0m
                }
            });
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("addMessage(message: Foo): Observable<void>", code);
        }

        [Fact]
        public async Task When_export_types_is_true_then_add_export_before_classes()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await generator.GenerateForControllerAsync<DiscussionController>();
            var json = document.ToJson();

            // Act
            var codeGen = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                Template = TypeScriptTemplate.Angular,
                GenerateClientInterfaces = true,
                TypeScriptGeneratorSettings =
                {
                    TypeScriptVersion = 2.0m,
                    ExportTypes = true
                }
            });
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("export class DiscussionClient", code);
            Assert.Contains("export interface IDiscussionClient", code);
        }

        [Fact]
        public async Task When_export_types_is_false_then_dont_add_export_before_classes()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await generator.GenerateForControllerAsync<DiscussionController>();
            var json = document.ToJson();

            // Act
            var codeGen = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                Template = TypeScriptTemplate.Angular,
                GenerateClientInterfaces = true,
                TypeScriptGeneratorSettings =
                {
                    TypeScriptVersion = 2.0m,
                    ExportTypes = false
                }
            });
            var code = codeGen.GenerateFile();

            // Assert
            Assert.DoesNotContain("export class DiscussionClient", code);
            Assert.DoesNotContain("export interface IDiscussionClient", code);
        }

        [Fact]
        public async Task When_generic_request()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await generator.GenerateForControllerAsync<DiscussionController>();
            var json = document.ToJson();

            // Act
            var codeGen = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                Template = TypeScriptTemplate.Angular,
                GenerateDtoTypes = true,
                TypeScriptGeneratorSettings =
                {
                    TypeScriptVersion = 2.7m,
                    ExportTypes = false
                }
            });
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("this.request = new RequestBodyBase()", code);
            Assert.Contains("this.request = new RequestBody()", code);
        }

        [Fact]
        public async Task When_consumes_is_url_encoded_then_construct_url_encoded_request()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await generator.GenerateForControllerAsync<UrlEncodedRequestConsumingController>();
            var json = document.ToJson();

            // Act
            var codeGen = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                Template = TypeScriptTemplate.Angular,
                TypeScriptGeneratorSettings =
                {
                    TypeScriptVersion = 2.0m
                }
            });
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("content_", code);
            Assert.DoesNotContain("FormData", code);
            Assert.Contains("\"Content-Type\": \"application/x-www-form-urlencoded\"", code);
        }
    }
}
