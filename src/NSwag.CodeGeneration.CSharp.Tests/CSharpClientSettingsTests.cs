using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Generation.WebApi;
using Xunit;

namespace NSwag.CodeGeneration.CSharp.Tests
{
    public class CSharpClientSettingsTests
    {
        public class FooController : Controller
        {
            public object GetPerson(bool @override = false)
            {
                return null;
            }
        }

        [Fact]
        public async Task When_ConfigurationClass_is_set_then_correct_ctor_is_generated()
        {
            // Arrange
            var swaggerGenerator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await swaggerGenerator.GenerateForControllerAsync<FooController>();

            var generator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings
            {
                InjectHttpClient = false,
                ConfigurationClass = "MyConfig",
                ClientBaseClass = "MyBaseClass"
            });

            // Act
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("public FooClient(MyConfig configuration) : base(configuration)", code);
        }

        [Fact]
        public async Task When_UseHttpRequestMessageCreationMethod_is_set_then_CreateRequestMessage_is_generated()
        {
            // Arrange
            var swaggerGenerator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await swaggerGenerator.GenerateForControllerAsync<FooController>();

            var generator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings
            {
                ConfigurationClass = "MyConfig",
                ClientBaseClass = "MyBaseClass",
                UseHttpRequestMessageCreationMethod = true
            });

            // Act
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("var request_ = await CreateHttpRequestMessageAsync(cancellationToken).ConfigureAwait(false)", code);
        }

        [Fact]
        public async Task When_parameter_name_is_reserved_keyword_then_it_is_appended_with_at()
        {
            // Arrange
            var swaggerGenerator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await swaggerGenerator.GenerateForControllerAsync<FooController>();

            var generator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings());

            // Act
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("Task<object> GetPersonAsync(bool? @override, ", code);
        }

        [Fact]
        public async Task When_code_is_generated_then_by_default_the_system_httpclient_is_used()
        {
            // Arrange
            var swaggerGenerator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await swaggerGenerator.GenerateForControllerAsync<FooController>();

            var generator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings
            {
                InjectHttpClient = false
            });

            // Act
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("var client_ = new System.Net.Http.HttpClient();", code);
        }

        [Fact]
        public async Task When_custom_http_client_type_is_specified_then_an_instance_of_that_type_is_used()
        {
            // Arrange
            var swaggerGenerator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await swaggerGenerator.GenerateForControllerAsync<FooController>();

            var generator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings
            {
                HttpClientType = "CustomNamespace.CustomHttpClient",
                InjectHttpClient = false
            });

            // Act
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("var client_ = new CustomNamespace.CustomHttpClient();", code);
        }

        [Fact]
        public async Task When_client_base_interface_is_not_specified_then_client_interface_should_have_no_base_interface()
        {
            // Arrange
            var swaggerGenerator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await swaggerGenerator.GenerateForControllerAsync<FooController>();

            var generator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings
            {
                GenerateClientInterfaces = true
            });

            // Act
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("public partial interface IFooClient\n", code);
        }

        [Fact]
        public async Task When_client_base_interface_is_specified_then_client_interface_extends_it()
        {
            // Arrange
            var swaggerGenerator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await swaggerGenerator.GenerateForControllerAsync<FooController>();

            var generator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings
            {
                GenerateClientInterfaces = true,
                ClientBaseInterface = "IClientBase"
            });

            // Act
            var code = generator.GenerateFile();

            // Assert
            Assert.Contains("public partial interface IFooClient : IClientBase", code);
        }
    }
}