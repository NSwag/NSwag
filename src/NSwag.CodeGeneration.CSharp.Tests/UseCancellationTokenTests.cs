using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.CodeGeneration.CSharp.Models;
using NSwag.Generation.WebApi;
using Xunit;

namespace NSwag.CodeGeneration.CSharp.Tests
{
    public class UseCancellationTokenTests
    {
        public class TestController : Controller
        {
            [Route("Foo")]
            public string Foo(string test, bool test2)
            {
                throw new NotImplementedException();
            }

            [Route("Bar")]
            public void Bar()
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public async Task When_controllerstyleispartial_and_usecancellationtokenistrue_and_requesthasnoparameter_then_cancellationtoken_is_added()
        {
            // Arrange
            var swaggerGen = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await swaggerGen.GenerateForControllerAsync<TestController>();

            // Act
            var codeGen = new CSharpControllerGenerator(document, new CSharpControllerGeneratorSettings
            {
                UseCancellationToken = true
            });
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("(System.Threading.CancellationToken cancellationToken)", code);
            Assert.Contains("_implementation.BarAsync(cancellationToken)", code);
            Assert.Contains("System.Threading.Tasks.Task BarAsync(" +
                "System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))", code);
        }

        [Fact]
        public async Task When_controllerstyleispartial_and_usecancellationtokenistrue_and_requesthasparameter_then_cancellationtoken_is_added()
        {
            // Arrange
            var swaggerGen = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await swaggerGen.GenerateForControllerAsync<TestController>();

            // Act
            var codeGen = new CSharpControllerGenerator(document, new CSharpControllerGeneratorSettings
            {
                UseCancellationToken = true
            });
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("System.Threading.Tasks.Task<string> FooAsync(string test, bool test2, " +
                "System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))", code);
            Assert.Contains("_implementation.FooAsync(test, test2, cancellationToken);", code);
            Assert.Contains("public System.Threading.Tasks.Task<string> Foo([Microsoft.AspNetCore.Mvc.FromQuery] string test, [Microsoft.AspNetCore.Mvc.FromQuery] bool test2, " +
                "System.Threading.CancellationToken cancellationToken)", code);
        }

        [Fact]
        public async Task When_controllerstyleisabstract_and_usecancellationtokenistrue_and_requesthasnoparameter_then_cancellationtoken_is_added()
        {
            // Arrange
            var swaggerGen = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await swaggerGen.GenerateForControllerAsync<TestController>();

            // Act
            var codeGen = new CSharpControllerGenerator(document, new CSharpControllerGeneratorSettings
            {
                ControllerStyle = CSharpControllerStyle.Abstract,
                UseCancellationToken = true
            });
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("abstract System.Threading.Tasks.Task Bar(" +
                "System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))", code);
        }

        [Fact]
        public async Task When_controllerstyleisabstract_and_usecancellationtokenistrue_and_requesthasparameter_then_cancellationtoken_is_added()
        {
            // Arrange
            var swaggerGen = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await swaggerGen.GenerateForControllerAsync<TestController>();

            // Act
            var codeGen = new CSharpControllerGenerator(document, new CSharpControllerGeneratorSettings
            {
                ControllerStyle = CSharpControllerStyle.Abstract,
                UseCancellationToken = true
            });
            var code = codeGen.GenerateFile();

            // Assert
            Assert.Contains("System.Threading.Tasks.Task<string> Foo([Microsoft.AspNetCore.Mvc.FromQuery] string test, [Microsoft.AspNetCore.Mvc.FromQuery] bool test2, " +
                "System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))", code);
        }

        [Fact]
        public async Task When_usecancellationtokenparameter_notsetted_then_cancellationtoken_isnot_added()
        {
            // Arrange
            var swaggerGen = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await swaggerGen.GenerateForControllerAsync<TestController>();

            // Act
            var codeGen = new CSharpControllerGenerator(document, new CSharpControllerGeneratorSettings
            {
                ControllerStyle = CSharpControllerStyle.Abstract
            });
            var code = codeGen.GenerateFile();

            // Assert
            Assert.DoesNotContain("System.Threading.CancellationToken cancellationToken", code);
        }
    }
}
