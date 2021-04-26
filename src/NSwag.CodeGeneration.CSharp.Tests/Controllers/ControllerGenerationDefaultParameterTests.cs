using System.Text.RegularExpressions;
using NJsonSchema;
using Xunit;

namespace NSwag.CodeGeneration.CSharp.Tests.Controllers
{
    public class ControllerGenerationDefaultParameterTests
    {
        [Fact]
        public void When_parameter_has_default_then_set_in_partial_controller()
        {
            // Arrange
            var document = new OpenApiDocument();
            document.Paths["foo/bar"] = new OpenApiPathItem
            {
                {
                    OpenApiOperationMethod.Get,
                    new OpenApiOperation {
                        Parameters = {
                            new OpenApiParameter {
                                Name = "booldef",
                                IsRequired = false,
                                Default = true,
                                Kind = OpenApiParameterKind.Query,
                                Type = JsonObjectType.Boolean
                            },
                            new OpenApiParameter {
                                Name = "intdef",
                                IsRequired = false,
                                Default = 42,
                                Kind = OpenApiParameterKind.Query,
                                Type = JsonObjectType.Integer
                            },
                            new OpenApiParameter {
                                Name = "bar",
                                IsRequired = false,
                                Kind = OpenApiParameterKind.Query,
                                Type = JsonObjectType.Integer
                            },
                            new OpenApiParameter {
                                Name = "doubledef",
                                IsRequired = false,
                                Default = 0.6822871999174,
                                Kind = OpenApiParameterKind.Query,
                                Type = JsonObjectType.Number
                            },
                            new OpenApiParameter {
                                Name = "decdef",
                                IsRequired = false,
                                Default = 79228162514264337593543950335M,
                                Kind = OpenApiParameterKind.Query,
                                Type = JsonObjectType.Number,
                                Format = JsonFormatStrings.Decimal
                            },
                            new OpenApiParameter {
                                Name = "abc",
                                IsRequired = true,
                                Default = 84,
                                Kind = OpenApiParameterKind.Query,
                                Type = JsonObjectType.Integer
                            },
                            new OpenApiParameter {
                                Name = "strdef",
                                Default = @"default""string""",
                                IsRequired = false,
                                Kind = OpenApiParameterKind.Query,
                                Type = JsonObjectType.String
                            }
                        }
                    }
                }
            };

            // Act
            var generator =
                new CSharpControllerGenerator(document,
                    new CSharpControllerGeneratorSettings { GenerateOptionalParameters = true });
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("_implementation.BarAsync(abc, booldef ?? true, intdef ?? 42, doubledef ?? 0.6822871999174D, decdef ?? 79228162514264337593543950335M, strdef ?? \"default\\\"string\\\"\", bar)", code);
            Assert.Contains("BarAsync(int abc, bool booldef, int intdef, double doubledef, decimal decdef, string strdef, int? bar = null);", code);

            var trimmedCode = RemoveExternalReferences(code);

            //CompilerParameters parameters = new CompilerParameters { GenerateInMemory = true };

            //var result = new CSharpCodeProvider().CompileAssemblyFromSource(parameters, trimmedCode);
            //if (result.Errors.Count > 0)
            //{
            //    foreach (var error in result.Errors)
            //    {
            //        Console.WriteLine(error.ToString());
            //    }
            //}

            //Assert.True(result.Errors.Count == 0);
        }

        private static string RemoveExternalReferences(string code)
        {
            // Remove the external references, and verify that the generated code compiles
            var trimmedCode = code.Replace(": .ApiController", "")
                .Replace("[.HttpGet, .Route(\"foo/bar\")]", "");

            trimmedCode = new Regex(@"\[System.CodeDom.Compiler.GeneratedCode.*]").Replace(trimmedCode, "");
            return trimmedCode;
        }
    }
}