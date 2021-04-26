using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Generation.WebApi;
using Xunit;

namespace NSwag.CodeGeneration.CSharp.Tests
{
    public class OptionalParameterTests
    {
        public class TestController : Controller
        {
            [Route("Test")]
            public void Test(string a, string b, string c = null)
            {
            }

            [Route("TestWithClass")]
            public void TestWithClass([FromUri] MyClass objet)
            {
            }

            [Route("TestWithEnum")]
            public void TestWithEnum([FromUri] MyEnum? myEnum = null)
            {
            }
        }

        public class FromUriAttribute : Attribute { }

        public enum MyEnum
        {
            One,
            Two,
            Three,
            Four

        }
        public class MyClass
        {
            private string MyString { get; set; }
            public MyEnum? MyEnum { get; set; }
            public int MyInt { get; set; }
        }

        [Fact]
        public async Task When_setting_is_enabled_with_enum_fromuri_should_make_enum_nullable()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await generator.GenerateForControllerAsync<TestController>();

            // Act
            var codeGenerator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings
            {
                GenerateOptionalParameters = true
            });
            var code = codeGenerator.GenerateFile();

            // Assert
            Assert.DoesNotContain("TestWithEnumAsync(MyEnum myEnum = null)", code);
            Assert.Contains("TestWithEnumAsync(MyEnum? myEnum = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))", code);
        }

        [Fact]
        public async Task When_setting_is_enabled_with_class_fromuri_should_make_enum_nullable()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await generator.GenerateForControllerAsync<TestController>();

            // Act
            var codeGenerator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings
            {
                GenerateOptionalParameters = true
            });
            var code = codeGenerator.GenerateFile();

            // Assert
            Assert.DoesNotContain("TestWithClassAsync(string myString = null, MyEnum myEnum = null, int? myInt = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))", code);
            Assert.Contains("TestWithClassAsync(string myString = null, MyEnum? myEnum = null, int? myInt = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))", code);
        }


        [Fact]
        public async Task When_setting_is_enabled_then_optional_parameters_have_null_optional_value()
        {
            // Arrange
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await generator.GenerateForControllerAsync<TestController>();

            // Act
            var codeGenerator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings
            {
                GenerateOptionalParameters = true
            });
            var code = codeGenerator.GenerateFile();

            // Assert
            Assert.Contains("TestAsync(string a, string b, string c = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))", code);
            Assert.DoesNotContain("TestAsync(string a, string b, string c)", code);
        }

        [Fact]
        public async Task When_setting_is_enabled_then_parameters_are_reordered()
        {
            var generator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await generator.GenerateForControllerAsync<TestController>();

            // Act
            var operation = document.Operations.First().Operation;
            var lastParameter = operation.Parameters.Last();
            operation.Parameters.Remove(lastParameter);
            operation.Parameters.Insert(0, lastParameter);
            var json = document.ToJson();

            var codeGenerator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings
            {
                GenerateOptionalParameters = true
            });
            var code = codeGenerator.GenerateFile();

            // Assert
            Assert.Contains("TestAsync(string a, string b, string c = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))", code);
        }
    }
}