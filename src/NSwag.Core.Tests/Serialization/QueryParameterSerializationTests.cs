using System.Threading.Tasks;
using Newtonsoft.Json;
using NJsonSchema;
using Xunit;

namespace NSwag.Core.Tests.Serialization
{
    public class QueryParameterSerializationTests
    {
        [Fact]
        public async Task When_style_is_form_then_explode_should_serialize_as_true_by_default()
        {
            //// Arrange
            var parameter = new OpenApiParameter();
            parameter.Name = "Foo";
            parameter.Kind = OpenApiParameterKind.Query;
            parameter.Style = OpenApiParameterStyle.Form;
            parameter.Schema = new JsonSchema
            {
                Type = JsonObjectType.Array,
                Item = new JsonSchema
                {
                    Type = JsonObjectType.String
                }
            };

            //// Act
            var json = parameter.ToJson(Formatting.Indented);

            //// Assert
            Assert.Equal(
                @"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""name"": ""Foo"",
  ""in"": ""query"",
  ""style"": ""form"",
  ""explode"": true,
  ""schema"": {
    ""type"": ""array"",
    ""items"": {
      ""type"": ""string""
    }
  }
}".Replace("\r", ""), json.Replace("\r", ""));
        }

        [Fact]
        public async Task When_style_is_form_then_explode_should_serialize_as_false_when_set()
        {
            //// Arrange
            var parameter = new OpenApiParameter();
            parameter.Name = "Foo";
            parameter.Kind = OpenApiParameterKind.Query;
            parameter.Style = OpenApiParameterStyle.Form;
            parameter.Explode = false;
            parameter.Schema = new JsonSchema
            {
                Type = JsonObjectType.Array,
                Item = new JsonSchema
                {
                    Type = JsonObjectType.String
                }
            };

            //// Act
            var json = parameter.ToJson(Formatting.Indented);

            //// Assert
            Assert.Equal(
                @"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""name"": ""Foo"",
  ""in"": ""query"",
  ""style"": ""form"",
  ""explode"": false,
  ""schema"": {
    ""type"": ""array"",
    ""items"": {
      ""type"": ""string""
    }
  }
}".Replace("\r", ""), json.Replace("\r", ""));
        }
    }
}