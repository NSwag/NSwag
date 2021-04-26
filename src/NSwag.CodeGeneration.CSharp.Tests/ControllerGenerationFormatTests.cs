using System.Collections.Generic;
using System.Text.RegularExpressions;
using NJsonSchema;
using NSwag.CodeGeneration.CSharp.Models;
using NSwag.CodeGeneration.OperationNameGenerators;
using Xunit;

namespace NSwag.CodeGeneration.CSharp.Tests
{
    public class ControllerGenerationFormatTests
    {
        [Fact]
        public void When_controllergenerationformat_abstract_then_abstractcontroller_is_generated()
        {
            // Arrange
            var document = GetOpenApiDocument();

            // Act
            var codeGen = new CSharpControllerGenerator(document, new CSharpControllerGeneratorSettings
            {
                ControllerStyle = CSharpControllerStyle.Abstract,
            });
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("abstract class TestController", code);
            Assert.DoesNotContain("ITestController", code);
            Assert.DoesNotContain("private ITestController _implementation;", code);
            Assert.DoesNotContain("partial class TestController", code);
        }

        [Fact]
        public void When_controllergenerationformat_partial_then_partialcontroller_is_generated()
        {
            // Arrange
            var document = GetOpenApiDocument();

            // Act
            var codeGen = new CSharpControllerGenerator(document, new CSharpControllerGeneratorSettings
            {
                ControllerStyle = CSharpControllerStyle.Partial,
            });
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("partial class TestController", code);
            Assert.Contains("ITestController", code);
            Assert.Contains("private ITestController _implementation;", code);
            Assert.DoesNotContain("abstract class TestController", code);
        }

        [Fact]
        public void When_aspnet_actiontype_inuse_with_abstract_then_actiontype_is_generated()
        {
            // Arrange
            var document = GetOpenApiDocument();

            // Act
            var codeGen = new CSharpControllerGenerator(document, new CSharpControllerGeneratorSettings
            {
                ControllerStyle = CSharpControllerStyle.Abstract,
                UseActionResultType = true
            });
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("public abstract System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.ActionResult<string>> Foo([Microsoft.AspNetCore.Mvc.FromQuery] string test, [Microsoft.AspNetCore.Mvc.FromQuery] bool? test2);", code);
            Assert.Contains("public abstract System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult> Bar();", code);
        }

        [Fact]
        public void When_aspnet_actiontype_inuse_with_partial_then_actiontype_is_generated()
        {
            // Arrange
            var document = GetOpenApiDocument();

            // Act
            var codeGen = new CSharpControllerGenerator(document, new CSharpControllerGeneratorSettings
            {
                ControllerStyle = CSharpControllerStyle.Partial,
                UseActionResultType = true
            });
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.ActionResult<string>> FooAsync(string test, bool? test2);", code);
            Assert.Contains("public System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.ActionResult<string>> Foo([Microsoft.AspNetCore.Mvc.FromQuery] string test, [Microsoft.AspNetCore.Mvc.FromQuery] bool? test2)", code);
            Assert.Contains("System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult> BarAsync();", code);
            Assert.Contains("public System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult> Bar()", code);
        }

        [Fact]
        public void When_controllergenerationformat_notsetted_then_partialcontroller_is_generated()
        {
            // Arrange
            var document = GetOpenApiDocument();

            // Act
            var codeGen = new CSharpControllerGenerator(document, new CSharpControllerGeneratorSettings());
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("partial class TestController", code);
            Assert.Contains("ITestController", code);
            Assert.Contains("private ITestController _implementation;", code);
            Assert.DoesNotContain("abstract class TestController", code);
        }

        [Fact]
        public void When_controller_has_operation_with_complextype_then_partialcontroller_is_generated_with_frombody_attribute()
        {
            // Arrange
            var document = GetOpenApiDocument();
            var settings = new CSharpControllerGeneratorSettings();

            // Act
            var codeGen = new CSharpControllerGenerator(document, settings);
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("partial class TestController", code);
            Assert.Contains($"Complex([Microsoft.AspNetCore.Mvc.FromBody] ComplexType complexType)", code);
            Assert.Contains("Foo([Microsoft.AspNetCore.Mvc.FromQuery] string test, [Microsoft.AspNetCore.Mvc.FromQuery] bool? test2)", code);
            Assert.Contains("Bar()", code);
        }

        [Fact]
        public void When_controller_has_operation_with_complextype_then_abstractcontroller_is_generated_with_frombody_attribute()
        {
            // Arrange
            var document = GetOpenApiDocument();
            var settings = new CSharpControllerGeneratorSettings
            {
                ControllerStyle = CSharpControllerStyle.Abstract,
            };

            // Act
            var codeGen = new CSharpControllerGenerator(document, settings);
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("abstract class TestController", code);
            Assert.Contains($"Complex([Microsoft.AspNetCore.Mvc.FromBody] ComplexType complexType)", code);
            Assert.Contains("Foo([Microsoft.AspNetCore.Mvc.FromQuery] string test, [Microsoft.AspNetCore.Mvc.FromQuery] bool? test2)", code);
            Assert.Contains("Bar()", code);
        }

        [Fact]
        public void When_controllerroutenamingstrategy_operationid_then_route_attribute_name_specified()
        {
            // Arrange
            var document = GetOpenApiDocument();
            var settings = new CSharpControllerGeneratorSettings
            {
                RouteNamingStrategy = CSharpControllerRouteNamingStrategy.OperationId
            };

            // Act
            var codeGen = new CSharpControllerGenerator(document, settings);
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("Route(\"Foo\", Name = \"Test_Foo\")", code);
            Assert.Contains("Route(\"Bar\", Name = \"Test_Bar\")", code);
        }

        [Fact]
        public void When_controllerroutenamingstrategy_none_then_route_attribute_name_not_specified()
        {
            // Arrange
            var document = GetOpenApiDocument();
            var settings = new CSharpControllerGeneratorSettings
            {
                RouteNamingStrategy = CSharpControllerRouteNamingStrategy.None
            };

            // Act
            var codeGen = new CSharpControllerGenerator(document, settings);
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("Route(\"Foo\")", code);
            Assert.Contains("Route(\"Bar\")", code);
        }

        [Fact]
        public void When_controller_has_operations_with_required_parameters_then_partialcontroller_is_generated_with_bindrequired_attribute()
        {
            // Arrange
            var document = GetOpenApiDocument();
            var settings = new CSharpControllerGeneratorSettings
            {
                GenerateModelValidationAttributes = true,
            };

            // Act
            var codeGen = new CSharpControllerGenerator(document, settings);
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("partial class TestController", code);
            Assert.Contains($"Complex([Microsoft.AspNetCore.Mvc.FromBody] ComplexType complexType)", code);
            Assert.Contains($"ComplexRequired([Microsoft.AspNetCore.Mvc.FromBody] [Microsoft.AspNetCore.Mvc.ModelBinding.BindRequired] ComplexType complexType)", code);
            Assert.Contains($"Foo([Microsoft.AspNetCore.Mvc.FromQuery] string test, [Microsoft.AspNetCore.Mvc.FromQuery] bool? test2)", code);
            Assert.Contains($"FooRequired([Microsoft.AspNetCore.Mvc.FromQuery] [Microsoft.AspNetCore.Mvc.ModelBinding.BindRequired] string test, [Microsoft.AspNetCore.Mvc.FromQuery] [Microsoft.AspNetCore.Mvc.ModelBinding.BindRequired] bool test2)", code);
            Assert.Contains($"HeaderParamRequired([Microsoft.AspNetCore.Mvc.FromHeader(Name = \"comes-from-header\")] [Microsoft.AspNetCore.Mvc.ModelBinding.BindRequired] string comes_from_header)", code);
            Assert.Contains("Bar()", code);
        }

        [Fact]
        public void When_controller_has_operations_with_required_parameters_then_abstractcontroller_is_generated_with_bindrequired_attribute()
        {
            // Arrange
            var document = GetOpenApiDocument();
            var settings = new CSharpControllerGeneratorSettings
            {
                ControllerStyle = CSharpControllerStyle.Abstract,
                GenerateModelValidationAttributes = true,
            };

            // Act
            var codeGen = new CSharpControllerGenerator(document, settings);
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("abstract class TestController", code);
            Assert.Contains($"Complex([Microsoft.AspNetCore.Mvc.FromBody] ComplexType complexType)", code);
            Assert.Contains($"ComplexRequired([Microsoft.AspNetCore.Mvc.FromBody] [Microsoft.AspNetCore.Mvc.ModelBinding.BindRequired] ComplexType complexType)", code);
            Assert.Contains($"Foo([Microsoft.AspNetCore.Mvc.FromQuery] string test, [Microsoft.AspNetCore.Mvc.FromQuery] bool? test2)", code);
            Assert.Contains($"FooRequired([Microsoft.AspNetCore.Mvc.FromQuery] [Microsoft.AspNetCore.Mvc.ModelBinding.BindRequired] string test, [Microsoft.AspNetCore.Mvc.FromQuery] [Microsoft.AspNetCore.Mvc.ModelBinding.BindRequired] bool test2)", code);
            Assert.Contains($"HeaderParamRequired([Microsoft.AspNetCore.Mvc.FromHeader(Name = \"comes-from-header\")] [Microsoft.AspNetCore.Mvc.ModelBinding.BindRequired] string comes_from_header)", code);
            Assert.Contains("Bar()", code);
        }

        [Fact]
        public void When_the_generation_of_dto_classes_are_disabled_then_file_is_generated_without_any_dto_clasess()
        {
            // Arrange
            var document = GetOpenApiDocument();
            var settings = new CSharpControllerGeneratorSettings
            {
                GenerateDtoTypes = false
            };

            // Act
            var codeGen = new CSharpControllerGenerator(document, settings);
            var code = codeGen.GenerateFile();

            // Assert
            Assert.DoesNotContain("public partial class ComplexType", code);
            Assert.DoesNotContain("public partial class ComplexTypeResponse", code);
        }

        private OpenApiDocument GetOpenApiDocument()
        {
            JsonSchema complexTypeSchema = new JsonSchema();
            complexTypeSchema.Title = "ComplexType";
            complexTypeSchema.Properties["Prop1"] = new JsonSchemaProperty { Type = JsonObjectType.String, IsRequired = true };
            complexTypeSchema.Properties["Prop2"] = new JsonSchemaProperty { Type = JsonObjectType.Integer, IsRequired = true };
            complexTypeSchema.Properties["Prop3"] = new JsonSchemaProperty { Type = JsonObjectType.Boolean, IsRequired = true };
            complexTypeSchema.Properties["Prop4"] = new JsonSchemaProperty { Type = JsonObjectType.Object, Reference = complexTypeSchema, IsRequired = true };

            JsonSchema complexTypeReponseSchema = new JsonSchema();
            complexTypeReponseSchema.Title = "ComplexTypeResponse";
            complexTypeReponseSchema.Properties["Prop1"] = new JsonSchemaProperty { Type = JsonObjectType.String, IsRequired = true };
            complexTypeReponseSchema.Properties["Prop2"] = new JsonSchemaProperty { Type = JsonObjectType.Integer, IsRequired = true };
            complexTypeReponseSchema.Properties["Prop3"] = new JsonSchemaProperty { Type = JsonObjectType.Boolean, IsRequired = true };
            complexTypeReponseSchema.Properties["Prop4"] = new JsonSchemaProperty { Type = JsonObjectType.Object, Reference = complexTypeSchema, IsRequired = true };

            var typeString = JsonSchema.FromType(typeof(string));

            var document = new OpenApiDocument();
            document.Paths["Foo"] = new OpenApiPathItem
            {
                {
                    OpenApiOperationMethod.Get,
                    new OpenApiOperation {
                        OperationId = "Test_Foo",
                        Parameters = {
                            new OpenApiParameter {
                                Name = "test",
                                IsRequired = false,
                                Kind = OpenApiParameterKind.Query,
                                Type = JsonObjectType.String
                            },
                            new OpenApiParameter {
                                Name = "test2",
                                IsRequired = false,
                                Kind = OpenApiParameterKind.Query,
                                Type = JsonObjectType.Boolean
                            }
                        },
                        Responses =
                        {
                            new KeyValuePair<string, OpenApiResponse>("200", new OpenApiResponse
                            {
                                Schema = typeString
                            })
                        }
                    }
                }
            };

            document.Paths["FooRequired"] = new OpenApiPathItem
            {
                {
                    OpenApiOperationMethod.Get,
                    new OpenApiOperation {
                        OperationId = "Test_FooRequired",
                        Parameters = {
                            new OpenApiParameter {
                                Name = "test",
                                IsRequired = true,
                                Kind = OpenApiParameterKind.Query,
                                Type = JsonObjectType.String
                            },
                            new OpenApiParameter {
                                Name = "test2",
                                IsRequired = true,
                                Kind = OpenApiParameterKind.Query,
                                Type = JsonObjectType.Boolean
                            }
                        },
                        Responses =
                        {
                            new KeyValuePair<string, OpenApiResponse>("200", new OpenApiResponse
                            {
                                Schema = typeString
                            })
                        }
                    }
                }
            };

            document.Paths["Bar"] = new OpenApiPathItem
            {
                {
                    OpenApiOperationMethod.Post,
                    new OpenApiOperation {
                        OperationId = "Test_Bar",
                    }
                }
            };

            document.Paths["HeaderParam"] = new OpenApiPathItem
            {
                {
                    OpenApiOperationMethod.Post,
                    new OpenApiOperation {
                        OperationId = "Test_HeaderParam",
                        Parameters = {
                            new OpenApiParameter {
                                Name = "comesFromHeader",
                                Kind = OpenApiParameterKind.Header,
                                Type = JsonObjectType.String
                            }
                        }
                    }
                }
            };

            document.Paths["HeaderParamRequired"] = new OpenApiPathItem
            {
                {
                    OpenApiOperationMethod.Post,
                    new OpenApiOperation {
                        OperationId = "Test_HeaderParamRequired",
                        Parameters = {
                            new OpenApiParameter {
                                Name = "comes-from-header",
                                IsRequired = true,
                                Kind = OpenApiParameterKind.Header,
                                Type = JsonObjectType.String
                            }
                        },
                        Tags = new List<string> { "Secondary" }
                    }
                }
            };

            document.Paths["Complex"] = new OpenApiPathItem
            {
                {
                    OpenApiOperationMethod.Post,
                    new OpenApiOperation {
                        OperationId = "Test_Complex",
                        Parameters = {
                            new OpenApiParameter {
                                Name = "complexType",
                                IsRequired = false,
                                Kind = OpenApiParameterKind.Body,
                                Type = JsonObjectType.Object,
                                Reference = complexTypeSchema
                            }
                        }
                    }
                }
            };

            document.Paths["ComplexRequired"] = new OpenApiPathItem
            {
                {
                    OpenApiOperationMethod.Post,
                    new OpenApiOperation {
                        OperationId = "Test_ComplexRequired",
                        Parameters = {
                            new OpenApiParameter {
                                Name = "complexType",
                                IsRequired = true,
                                Kind = OpenApiParameterKind.Body,
                                Type = JsonObjectType.Object,
                                Reference = complexTypeSchema
                            }
                        },
                        Responses = {
                            new System.Collections.Generic.KeyValuePair<string, OpenApiResponse>
                            (
                                "200", new OpenApiResponse
                                {
                                    Reference = new OpenApiResponse
                                    {
                                        Schema = complexTypeReponseSchema
                                    }
                                }
                            )
                        }
                    }
                }
            };

            document.Definitions["ComplexType"] = complexTypeSchema;
            document.Definitions["ComplexTypeResponse"] = complexTypeReponseSchema;
            return document;
        }

        [Fact]
        public void When_controllertarget_aspnet_and_multiple_controllers_then_only_single_custom_fromheader_generated()
        {
            // Arrange
            var document = GetOpenApiDocument();
            var settings = new CSharpControllerGeneratorSettings
            {
                ControllerTarget = CSharpControllerTarget.AspNet,
                ControllerStyle = CSharpControllerStyle.Abstract,
                OperationNameGenerator = new MultipleClientsFromFirstTagAndPathSegmentsOperationNameGenerator()
            };

            // Act
            var codeGen = new CSharpControllerGenerator(document, settings);
            var code = codeGen.GenerateFile();

            // Assert
            var fromHeaderCustomAttributeCount = Regex.Matches(code, "public class FromHeaderAttribute :").Count;
            Assert.Equal(1, fromHeaderCustomAttributeCount);
            var fromHeaderCustomBindingCount = Regex.Matches(code, "public class FromHeaderBinding :").Count;
            Assert.Equal(1, fromHeaderCustomBindingCount);
            Assert.Contains("[FromHeader", code);
            Assert.DoesNotContain("[Microsoft.AspNetCore.Mvc.FromHeader", code);
        }

        [Fact]
        public void When_controllertarget_aspnetcore_then_use_builtin_fromheader()
        {
            // Arrange
            var document = GetOpenApiDocument();
            var settings = new CSharpControllerGeneratorSettings
            {
                ControllerTarget = CSharpControllerTarget.AspNetCore
            };

            // Act
            var codeGen = new CSharpControllerGenerator(document, settings);
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("[Microsoft.AspNetCore.Mvc.FromHeader", code);
            Assert.DoesNotContain("[FromHeader", code);
            Assert.DoesNotContain("public class FromHeaderBinding :", code);
            Assert.DoesNotContain("public class FromHeaderAttribute :", code);
        }

        [Fact]
        public void When_controller_has_operation_with_header_parameter_then_partialcontroller_is_generated_with_fromheader_attribute()
        {
            // Arrange
            var document = GetOpenApiDocument();
            var settings = new CSharpControllerGeneratorSettings
            {
                ControllerTarget = CSharpControllerTarget.AspNet
            };

            // Act
            var codeGen = new CSharpControllerGenerator(document, settings);
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("partial class TestController", code);
            Assert.Contains($"HeaderParam([FromHeader] string comesFromHeader)", code);
        }
    }
}
