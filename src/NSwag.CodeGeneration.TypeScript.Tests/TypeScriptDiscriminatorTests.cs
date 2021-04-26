using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NJsonSchema.CodeGeneration.TypeScript;
using NJsonSchema.Converters;
using NSwag.Generation.WebApi;
using System.Runtime.Serialization;
using Xunit;

namespace NSwag.CodeGeneration.TypeScript.Tests
{
    public class TypeScriptDiscriminatorTests
    {
        [JsonConverter(typeof(JsonInheritanceConverter), "type")]
        [KnownType(typeof(OneChild))]
        [KnownType(typeof(SecondChild))]
        public abstract class Base
        {
            public EBase Type { get; }
        }

        public enum EBase
        {
            OneChild,
            SecondChild
        }

        public class OneChild : Base
        {
            public string A { get; }
        }

        public class SecondChild : Base
        {
            public string B { get; }
        }

        public class Nested
        {
            public Base Child { get; set; }
        }

        public class DiscriminatorController
        {
            [Route("foo")]
            public string TestLeaf(Base param)
            {
                return null;
            }
            
            [Route("bar")]
            public string Test(OneChild param)
            {
                return null;
            }
            
            [Route("baz")]
            public string TestNested(Nested param)
            {
                return null;
            }
        }

        [Fact]
        public async Task When_parameter_is_abstract_then_generate_union()
        {
            // Arrange
            var swaggerGenerator = new WebApiOpenApiDocumentGenerator(new WebApiOpenApiDocumentGeneratorSettings());
            var document = await swaggerGenerator.GenerateForControllerAsync<DiscriminatorController>();
            var clientGenerator = new TypeScriptClientGenerator(document, new TypeScriptClientGeneratorSettings
            {
                TypeScriptGeneratorSettings =
                {
                    UseLeafType = true,
                    TypeScriptVersion = 1.4m,
                    NullValue = TypeScriptNullValue.Undefined
                }
            });

            var json = document.ToJson();

            // Act
            var code = clientGenerator.GenerateFile();

            // Assert
            Assert.Contains("test(param: OneChild)", code);
            Assert.Contains("testLeaf(param: OneChild | SecondChild)", code);
            Assert.Contains("child?: OneChild | SecondChild;", code);
        }
    }
}
