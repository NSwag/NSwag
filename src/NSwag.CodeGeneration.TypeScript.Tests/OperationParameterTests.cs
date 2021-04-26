using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NJsonSchema;
using NSwag.Generation.WebApi;
using System.Collections.Generic;
using Xunit;

namespace NSwag.CodeGeneration.TypeScript.Tests
{
    public class OperationParameterTests
    {
        public class FooController : Controller
        {
            [Route("foos/")]
            public Foo[] GetFoos([FromUri] Bar[] bars)
            {
                return new Foo[0];
            }
        }

        public class FromUriAttribute : Attribute { }

        public enum Bar
        {
            Baz,
            Foo
        }

        public class Foo
        {
            public Bar Bar { get; set; }

            public Bar Bar2 { get; set; }
        }

        [Fact]
        public async Task When_query_parameter_is_enum_array_then_the_enum_is_referenced()
        {
            var serializerSettings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> {new StringEnumConverter()}
            };

            // Arrange
            var settings = new WebApiOpenApiDocumentGeneratorSettings
            {
                DefaultUrlTemplate = "api/{controller}/{action}/{id}",
                SerializerSettings = serializerSettings,
                SchemaType = SchemaType.Swagger2,
            };
            var generator = new WebApiOpenApiDocumentGenerator(settings);

            // Act
            var document = await generator.GenerateForControllerAsync<FooController>();
            var json = document.ToJson();

            var clientSettings = new TypeScriptClientGeneratorSettings
            {
                Template = TypeScriptTemplate.JQueryCallbacks
            };
            clientSettings.TypeScriptGeneratorSettings.TypeScriptVersion = 1.8m;

            var gen = new TypeScriptClientGenerator(document, clientSettings);
            var code = gen.GenerateFile();

            // Assert
            Assert.NotNull(document.Operations.First().Operation.Parameters.First().Item.Reference);
            Assert.Contains("getFoos(bars: Bar[], ", code);
        }
    }
}
