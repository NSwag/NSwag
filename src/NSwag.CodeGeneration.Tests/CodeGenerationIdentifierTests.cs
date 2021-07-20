using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.CodeGeneration.TypeScript;
using NJsonSchema.Generation;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.OperationNameGenerators;
using NSwag.CodeGeneration.TypeScript;
using Xunit;

namespace NSwag.CodeGeneration.Tests
{
    public class CodeGenerationIdentifierTests
    {
        [Fact]
        public void When_generating_CSharp_code_with_MultipleClientsFromFirstTagAndOperationIdGenerator_then_valid_operation_name_generated()
        {
            // Arrange
            var document = CreateDocument();

            //// Act
            var settings = new CSharpClientGeneratorSettings();
            settings.OperationNameGenerator = new MultipleClientsFromFirstTagAndOperationIdGenerator();

            var generator = new CSharpClientGenerator(document, settings);
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("public partial class SomeTagHereClient", code);
            Assert.Contains("Task<Person> OperationIdAsync()", code);
        }

        [Fact]
        public void When_generating_CSharp_code_with_MultipleClientsFromFirstTagAndPathSegmentsOperationNameGenerator_then_valid_operation_name_generated()
        {
            // Arrange
            var document = CreateDocument();

            //// Act
            var settings = new CSharpClientGeneratorSettings();
            settings.OperationNameGenerator = new MultipleClientsFromFirstTagAndPathSegmentsOperationNameGenerator();

            var generator = new CSharpClientGenerator(document, settings);
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("public partial class SomeTagHereClient", code);
            Assert.Contains("Task<Person> PersonActionFooAsync()", code);
        }

        [Fact]
        public void When_generating_CSharp_code_with_MultipleClientsFromOperationIdOperationNameGenerator_then_valid_operation_name_generated()
        {
            // Arrange
            var document = CreateDocument();

            //// Act
            var settings = new CSharpClientGeneratorSettings();
            settings.OperationNameGenerator = new MultipleClientsFromOperationIdOperationNameGenerator();

            var generator = new CSharpClientGenerator(document, settings);
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("public partial class Client", code);
            Assert.Contains("Task<Person> OperationIdAsync()", code);
        }

        [Fact]
        public void When_generating_CSharp_code_with_MultipleClientsFromPathSegmentsOperationNameGenerator_then_valid_operation_name_generated()
        {
            // Arrange
            var document = CreateDocument();

            //// Act
            var settings = new CSharpClientGeneratorSettings();
            settings.OperationNameGenerator = new MultipleClientsFromPathSegmentsOperationNameGenerator();

            var generator = new CSharpClientGenerator(document, settings);
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("public partial class Client", code);
            Assert.Contains("Task<Person> PersonActionFooAsync()", code);
        }

        [Fact]
        public void When_generating_CSharp_code_with_SingleClientFromOperationIdOperationNameGenerator_then_valid_operation_name_generated()
        {
            // Arrange
            var document = CreateDocument();

            //// Act
            var settings = new CSharpClientGeneratorSettings();
            settings.OperationNameGenerator = new SingleClientFromOperationIdOperationNameGenerator();

            var generator = new CSharpClientGenerator(document, settings);
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("public partial class Client", code);
            Assert.Contains("Task<Person> OperationIdAsync()", code);
        }

        [Fact]
        public void When_generating_CSharp_code_with_SingleClientFromPathSegmentsOperationNameGenerator_then_valid_operation_name_generated()
        {
            // Arrange
            var document = CreateDocument();

            //// Act
            var settings = new CSharpClientGeneratorSettings();
            settings.OperationNameGenerator = new SingleClientFromPathSegmentsOperationNameGenerator();

            var generator = new CSharpClientGenerator(document, settings);
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("public partial class Client", code);
            Assert.Contains("Task<Person> PersonactionfooAsync()", code);
        }

        private static OpenApiDocument CreateDocument()
        {
            var settings = new JsonSchemaGeneratorSettings();
            var generator = new JsonSchemaGenerator(settings);

            var document = new OpenApiDocument();
            document.Paths["$/Person$?~Action=Foo$"] = new OpenApiPathItem();
            document.Paths["$/Person$?~Action=Foo$"][OpenApiOperationMethod.Get] = new OpenApiOperation
            {
                Responses =
                {
                    {
                        "200", new OpenApiResponse
                        {
                            Schema = new JsonSchema
                            {
                                Reference = generator.Generate(typeof(Person), new OpenApiSchemaResolver(document, settings))
                            }
                        }
                    }
                },
                Tags =
                {
                    "$Some$Tag|Here",
                },
                OperationId = "$Op$erat|ionId$",
            };
            return document;
        }
    }
}
