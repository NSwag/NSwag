using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NJsonSchema.CodeGeneration.TypeScript;
using NSwag.Generation.WebApi;
using Xunit;

namespace NSwag.CodeGeneration.TypeScript.Tests
{
    public class TypeScriptOperationParameterTests
    {
        public class NullableParameterController
        {
            [Route("foo")]
            public string Test(int a, int? b)
            {
                return null;
            }
        }

        public class NullableOptionalParameterController
        {
            [Route("foo")]
            public string Test(int a, int? b = null)
            {
                return null;
            }
        }

        [Fact]
        public async Task When_parameter_is_nullable_and_ts20_then_it_is_a_union_type_with_undefined()
        {
            // Arrange
            var swaggerGenerator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await swaggerGenerator.GenerateForControllerAsync<NullableParameterController>();
            var clientGenerator = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                TypeScriptGeneratorSettings =
                {
                    TypeScriptVersion = 2.0m, 
                    NullValue = TypeScriptNullValue.Undefined
                }
            });

            var json = document.ToJson();

            // Act
            var code = clientGenerator.GenerateFile();

            // Assert
            Assert.Contains("test(a: number, b: number | null)", code);
        }

        [Fact]
        public async Task When_parameter_is_nullable_and_ts20_then_it_is_not_included_in_query_string()
        {
            // Arrange
            var swaggerGenerator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await swaggerGenerator.GenerateForControllerAsync<NullableParameterController>();
            var clientGenerator = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                TypeScriptGeneratorSettings =
                {
                    TypeScriptVersion = 2.0m,
                    NullValue = TypeScriptNullValue.Undefined
                }
            });

            var json = document.ToJson();

            // Act
            var code = clientGenerator.GenerateFile();

            // Assert
            Assert.Contains("else if(b !== null)", code);
        }

        [Fact]
        public async Task When_parameter_is_nullable_optional_and_ts20_then_it_is_a_union_type_with_undefined()
        {
            // Arrange
            var swaggerGenerator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await swaggerGenerator.GenerateForControllerAsync<NullableOptionalParameterController>();
            var clientGenerator = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                TypeScriptGeneratorSettings =
                {
                    TypeScriptVersion = 2.0m,
                    NullValue = TypeScriptNullValue.Undefined
                }
            });

            var json = document.ToJson();

            // Act
            var code = clientGenerator.GenerateFile();

            // Assert
            Assert.Contains("test(a: number, b: number | null | undefined)", code);
        }

        [Fact]
        public async Task When_parameter_is_nullable_optional_and_ts20_then_it_is_not_included_in_query_string()
        {
            // Arrange
            var swaggerGenerator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await swaggerGenerator.GenerateForControllerAsync<NullableOptionalParameterController>();
            var clientGenerator = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                TypeScriptGeneratorSettings =
                {
                    TypeScriptVersion = 2.0m,
                    NullValue = TypeScriptNullValue.Undefined
                }
            });

            var json = document.ToJson();

            // Act
            var code = clientGenerator.GenerateFile();

            // Assert
            Assert.Contains("if (b !== undefined && b !== null)", code);
        }
    }
}
