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
    public class CodeGenerationTests
    {
        [Fact]
        public void When_generating_CSharp_code_then_output_contains_expected_classes()
        {
            // Arrange
            var document = CreateDocument();

            // Act
            var settings = new CSharpClientGeneratorSettings { ClassName = "MyClass" };
            settings.CSharpGeneratorSettings.Namespace = "MyNamespace";

            var generator = new CSharpClientGenerator(document, settings);
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("namespace MyNamespace", code);
            Assert.Contains("class MyClass", code);
            Assert.Contains("class Person", code);
            Assert.Contains("class Address", code);
        }

        [Fact]
        public void When_generating_CSharp_code_with_SystemTextJson_then_output_contains_expected_code()
        {
            // Arrange
            var document = CreateDocument();

            // Act
            var settings = new CSharpClientGeneratorSettings();
            settings.CSharpGeneratorSettings.JsonLibrary = NJsonSchema.CodeGeneration.CSharp.CSharpJsonLibrary.SystemTextJson;

            var generator = new CSharpClientGenerator(document, settings);
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("new System.Text.Json.JsonSerializerOptions()", code);
        }

        [Fact]
        public void When_generating_CSharp_code_with_SystemTextJson_and_JsonSerializerSettingsTransformationMethod_then_output_contains_expected_code()
        {
            // Arrange
            var document = CreateDocument();

            // Act
            var settings = new CSharpClientGeneratorSettings();
            settings.CSharpGeneratorSettings.JsonLibrary = NJsonSchema.CodeGeneration.CSharp.CSharpJsonLibrary.SystemTextJson;
            settings.CSharpGeneratorSettings.JsonSerializerSettingsTransformationMethod = "TestJsonSerializerSettingsTransformationMethod";

            var generator = new CSharpClientGenerator(document, settings);
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("TestJsonSerializerSettingsTransformationMethod(new System.Text.Json.JsonSerializerOptions())", code);
        }

        [Fact]
        public void When_generating_CSharp_code_with_NewtonsoftJson_and_JsonSerializerSettingsTransformationMethod_then_output_contains_expected_code()
        {
            // Arrange
            var document = CreateDocument();

            // Act
            var settings = new CSharpClientGeneratorSettings();
            settings.CSharpGeneratorSettings.JsonLibrary = NJsonSchema.CodeGeneration.CSharp.CSharpJsonLibrary.NewtonsoftJson;
            settings.CSharpGeneratorSettings.JsonSerializerSettingsTransformationMethod = "TestJsonSerializerSettingsTransformationMethod";

            var generator = new CSharpClientGenerator(document, settings);
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("TestJsonSerializerSettingsTransformationMethod(new Newtonsoft.Json.JsonSerializerSettings {  })", code);
        }

        [Fact]
        public void When_generating_CSharp_code_with_SystemTextJson_and_JsonConverters_then_output_contains_expected_code()
        {
            // Arrange
            var document = CreateDocument();

            // Act
            var settings = new CSharpClientGeneratorSettings();
            settings.CSharpGeneratorSettings.JsonLibrary = NJsonSchema.CodeGeneration.CSharp.CSharpJsonLibrary.SystemTextJson;
            settings.CSharpGeneratorSettings.JsonConverters = new[] { "CustomConverter1", "CustomConverter2" };

            var generator = new CSharpClientGenerator(document, settings);
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("new System.Text.Json.JsonSerializerOptions();", code);
            Assert.Contains("var converters = new System.Text.Json.Serialization.JsonConverter[] { new CustomConverter1(), new CustomConverter2() }", code);
            Assert.Contains("foreach(var converter in converters)", code);
            Assert.Contains("settings.Converters.Add(converter)", code);
        }

        [Fact]
        public void When_generating_CSharp_code_with_SystemTextJson_and_GenerateJsonMethods_and_JsonConverters_then_ToJson_and_FromJson_contains_expected_code()
        {
            // Arrange
            var document = CreateDocument();

            var expectedToJson = @"
public string ToJson()
{
	var options = new System.Text.Json.JsonSerializerOptions();
	var converters = new System.Text.Json.Serialization.JsonConverter[] { new CustomConverter1(), new CustomConverter2() };
	foreach(var converter in converters)
		options.Converters.Add(converter);
	return System.Text.Json.JsonSerializer.Serialize(this, options);
}";

            var expectedFromJson = @"
public static Person FromJson(string data)
{
	var options = new System.Text.Json.JsonSerializerOptions();
	var converters = new System.Text.Json.Serialization.JsonConverter[] { new CustomConverter1(), new CustomConverter2() };
	foreach(var converter in converters)
		options.Converters.Add(converter);
	return System.Text.Json.JsonSerializer.Deserialize<Person>(data, options);
}";

            // Act
            var settings = new CSharpClientGeneratorSettings();
            settings.CSharpGeneratorSettings.JsonLibrary = NJsonSchema.CodeGeneration.CSharp.CSharpJsonLibrary.SystemTextJson;
            settings.CSharpGeneratorSettings.JsonConverters = new[] { "CustomConverter1", "CustomConverter2" };
            settings.CSharpGeneratorSettings.GenerateJsonMethods = true;

            var generator = new CSharpClientGenerator(document, settings);
            var code = generator.GenerateFile();
            var normalizedCode = Regex.Replace(code, @"\s+", string.Empty);

            // Assert
            Assert.Contains(Regex.Replace(expectedToJson, @"\s+", string.Empty), normalizedCode);
            Assert.Contains(Regex.Replace(expectedFromJson, @"\s+", string.Empty), normalizedCode);
        }

        [Fact]
        public void When_generating_TypeScript_code_then_output_contains_expected_classes()
        {
            // Arrange
            var document = CreateDocument();

            // Act
            var generator = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                ClassName = "MyClass",
                TypeScriptGeneratorSettings =
                {
                    TypeStyle = TypeScriptTypeStyle.Interface
                }
            });
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("export class MyClass", code);
            Assert.Contains("export interface Person", code);
            Assert.Contains("export interface Address", code);
        }

        [Fact]
        public async Task When_using_json_schema_with_references_in_service_then_references_are_correctly_resolved()
        {
            // Arrange
            var jsonSchema = @"{
  ""definitions"": {
    ""app"": {
      ""definitions"": {
        ""name"": {
          ""pattern"": ""^[a-z][a-z0-9-]{3,30}$"",
          ""type"": ""string""
        }
      },
      ""properties"": {
        ""name"": {
          ""$ref"": ""#/definitions/app/definitions/name""
        }
      },
      ""required"": [""name""],
      ""type"": ""object""
    }
  },
  ""properties"": {
    ""app"": {
      ""$ref"": ""#/definitions/app""
    },
  },
  ""type"": ""object""
}";

            // Act
            var schema = await JsonSchema.FromJsonAsync(jsonSchema);
            var document = new OpenApiDocument();
            document.Definitions["Foo"] = schema;

            // Assert
            var jsonService = document.ToJson(); // no exception expected
        }

        [Fact]
        public void No_Brackets_in_Operation_Name()
        {
            // Arrange
            var path = "/my/path/with/{parameter_with_underscore}/and/{another_parameter}";

            // Act
            var operationName = SingleClientFromPathSegmentsOperationNameGenerator.ConvertPathToName(path);

            // Assert
            Assert.DoesNotContain("{", operationName);
            Assert.DoesNotContain("}", operationName);
            Assert.False(string.IsNullOrWhiteSpace(operationName));
        }

        private static OpenApiDocument CreateDocument()
        {
            var document = new OpenApiDocument();
            var settings = new JsonSchemaGeneratorSettings();
            var generator = new JsonSchemaGenerator(settings);

            document.Paths["/Person"] = new OpenApiPathItem();
            document.Paths["/Person"][OpenApiOperationMethod.Get] = new OpenApiOperation
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
                }
            };
            return document;
        }
    }

    public class Person
    {
        [Required]
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime Birthday { get; set; }

        public Sex Sex { get; set; }

        public Address Address { get; set; }
    }

    public class Address
    {
        public string Street { get; set; }

        public string City { get; set; }
    }

    public enum Sex
    {
        Male,
        Female
    }
}
