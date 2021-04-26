using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NJsonSchema;
using NSwag.Generation.WebApi;
using Xunit;

namespace NSwag.CodeGeneration.CSharp.Tests
{
    public class FormParameterTests
    {
        [Fact]
        public void When_form_parameters_are_defined_then_MultipartFormDataContent_is_generated()
        {
            // Arrange
            var document = new OpenApiDocument();
            document.Paths["foo/bar"] = new OpenApiPathItem
            {
                {
                    OpenApiOperationMethod.Post,
                    new OpenApiOperation
                    {
                        Parameters =
                        {
                            new OpenApiParameter
                            {
                                Name = "foo",
                                IsRequired = false,
                                IsNullableRaw = true,
                                Kind = OpenApiParameterKind.FormData,
                                Type = JsonObjectType.String
                            },
                            new OpenApiParameter
                            {
                                Name = "bar",
                                IsRequired = true,
                                IsNullableRaw = false,
                                Kind = OpenApiParameterKind.FormData,
                                Type = JsonObjectType.String
                            }
                        }
                    }
                }
            };

            // Act
            var generator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings());
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("new System.Net.Http.MultipartFormDataContent", code);
            Assert.Contains("if (foo != null)", code);
            Assert.Contains("throw new System.ArgumentNullException(\"bar\");", code);
        }

        public class FileUploadController : Controller
        {
            public void Upload(HttpPostedFileBase file)
            {
            }
        }

        public class HttpPostedFileBase
        {
        }

        [Fact]
        public async Task When_action_has_file_parameter_then_Stream_is_generated_in_CSharp_code()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await generator.GenerateForControllerAsync<FileUploadController>();

            // Act
            var codeGen = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings());
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("FileParameter file", code);
            Assert.Contains("var content_file_ = new System.Net.Http.StreamContent(file.Data);", code);
            Assert.Contains("content_.Add(content_file_, \"file\", file.FileName ??", code);
        }

        [Fact]
        public void When_form_parameters_are_defined_then_FormUrlEncodedContent_is_generated()
        {
            // Arrange
            var document = new OpenApiDocument();
            document.Paths["foo/bar"] = new OpenApiPathItem
            {
                {
                    OpenApiOperationMethod.Post,
                    new OpenApiOperation
                    {
                        Consumes = new System.Collections.Generic.List<string> { "application/x-www-form-urlencoded" },
                        Parameters =
                        {
                            new OpenApiParameter
                            {
                                Name = "foo",
                                IsRequired = false,
                                IsNullableRaw = true,
                                Kind = OpenApiParameterKind.FormData,
                                Type = JsonObjectType.String
                            },
                            new OpenApiParameter
                            {
                                Name = "bar",
                                IsRequired = true,
                                IsNullableRaw = false,
                                Kind = OpenApiParameterKind.FormData,
                                Type = JsonObjectType.String
                            }
                        }
                    }
                }
            };

            // Act
            var generator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings());
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("new System.Net.Http.FormUrlEncodedContent", code);
            Assert.Contains("if (foo != null)", code);
            Assert.Contains("throw new System.ArgumentNullException(\"bar\");", code);
        }

        // TODO: Implement for JQuery, AngularJS and Angular 2

        //[Fact]
        //public void When_action_has_file_parameter_then_Stream_is_generated_in_TypeScript_code()
        //{
        //    // Arrange
        //    var generator = new SwaggerGenerators.WebApi.WebApiToSwaggerGenerator(new WebApiToSwaggerGeneratorSettings());
        //    var service = generator.GenerateForController<FileUploadController>();

        //    // Act
        //    var codeGen = new SwaggerToTypeScriptClientGenerator(service, new SwaggerToTypeScriptClientGeneratorSettings());
        //    var code = codeGen.GenerateFile();

        //    // Assert
        //    Assert.True(code.Contains("Stream file"));
        //    Assert.True(code.Contains("var content_ = new MultipartFormDataContent();"));
        //    Assert.True(code.Contains("content_.Add(new StreamContent(file), \"file\");"));
        //}
    }
}