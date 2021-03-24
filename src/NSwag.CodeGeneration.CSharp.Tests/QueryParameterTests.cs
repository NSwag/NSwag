using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace NSwag.CodeGeneration.CSharp.Tests
{
    public class QueryParameterTests
    {
        [Fact]
        public void When_query_parameter_is_set_to_explode_and_style_is_form_object_parameters_are_expanded()
        {
            var spec = @"{
  ""openapi"": ""3.0.0"",
  ""info"": {
    ""version"": ""1.0.0"",
    ""title"": ""Query params tests""
  },
  ""servers"": [
    {
      ""url"": ""http://localhost:8080""
    }
  ],
  ""paths"": {
    ""/settings"": {
      ""get"": {
        ""summary"": ""List all settings"",
        ""operationId"": ""listSettings"",
        ""parameters"": [
          {
            ""name"": ""paging"",
            ""in"": ""query"",
            ""description"": ""list setting filter"",
            ""required"": false,
            ""style"": ""form"",
            ""explode"": true,
            ""schema"": {
              ""$ref"": ""#/components/schemas/Paging""
            }
          }
        ],
        ""responses"": {
          ""200"": {
            ""description"": ""A paged array of settings""
          }
        }
      }
    }
  },
  ""components"": {
    ""schemas"": {
      ""Paging"": {
        ""type"": ""object"",
        ""properties"": {
          ""page"": {
            ""type"": ""integer"",
            ""format"": ""int32""
          },
          ""limit"": {
            ""type"": ""integer"",
            ""format"": ""int32""
          }
        }
      }
    }
  }
}
";

            var document = OpenApiDocument.FromJsonAsync(spec).Result;

            //// Act
            var generator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings());
            var code = generator.GenerateFile();

            //// Assert
            Assert.Contains(
                "urlBuilder_.Append(System.Uri.EscapeDataString(\"page\") + \"=\").Append(System.Uri.EscapeDataString(ConvertToString(paging.Page, System.Globalization.CultureInfo.InvariantCulture))).Append(\"&\");",
                code);
            Assert.Contains(
                "urlBuilder_.Append(System.Uri.EscapeDataString(\"limit\") + \"=\").Append(System.Uri.EscapeDataString(ConvertToString(paging.Limit, System.Globalization.CultureInfo.InvariantCulture))).Append(\"&\");",
                code);
        }

        [Theory]
        [InlineData("form", ",")]
        [InlineData("spaceDelimited", "%20")]
        [InlineData("pipeDelimited", "|")]
        public void When_query_parameter_is_set_to_not_explode_style_should_be_respected_and_array_parameters_delimited(string style, string delimiter)
        {
            var spec = $@"{{
  ""openapi"": ""3.0.0"",
  ""info"": {{
    ""version"": ""1.0.0"",
    ""title"": ""Query params tests""
  }},
  ""servers"": [
    {{
      ""url"": ""http://localhost:8080""
    }}
  ],
  ""paths"": {{
    ""/settings"": {{
      ""get"": {{
        ""summary"": ""List all settings"",
        ""operationId"": ""listSettings"",
        ""parameters"": [
          {{
            ""name"": ""paging"",
            ""in"": ""query"",
            ""description"": ""list setting filter"",
            ""required"": false,
            ""style"": ""{style}"",
            ""explode"": false,
            ""type"": ""array"",
            ""items"": {{
              ""type"": ""integer"",
              ""format"": ""int32""
            }}
          }}
        ],
        ""responses"": {{
          ""200"": {{
            ""description"": ""A paged array of settings""
          }}
        }}
      }}
    }}
  }}
}}
";



            var document = OpenApiDocument.FromJsonAsync(spec).Result;

            //// Act
            var generator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings());
            var code = generator.GenerateFile();

            //// Assert

            var expected =
                $@"{{
    bool first_ = true;
    foreach (var item_ in paging)
    {{
        if (first_)
        {{
            urlBuilder_.Append(System.Uri.EscapeDataString(""paging"")).Append('=');
            first_ = false;
        }}
        else
            urlBuilder_.Append({(delimiter.Length == 1 ? $"'{delimiter}'" : $"\"{delimiter}\"")});

        urlBuilder_.Append(System.Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture)));
    }}
    if (!first_) 
        urlBuilder_.Append('&');
}}";

            AssertCodeContains(expected, code);
        }

        [Theory]
        [InlineData("form")]
        [InlineData("spaceDelimited")]
        [InlineData("pipeDelimited")]
        public void When_query_parameter_is_set_to_explode_style_is_ignored_and_array_parameters_are_exploded(string style)
        {
            var spec = $@"{{
  ""openapi"": ""3.0.0"",
  ""info"": {{
    ""version"": ""1.0.0"",
    ""title"": ""Query params tests""
  }},
  ""servers"": [
    {{
      ""url"": ""http://localhost:8080""
    }}
  ],
  ""paths"": {{
    ""/settings"": {{
      ""get"": {{
        ""summary"": ""List all settings"",
        ""operationId"": ""listSettings"",
        ""parameters"": [
          {{
            ""name"": ""paging"",
            ""in"": ""query"",
            ""description"": ""list setting filter"",
            ""required"": false,
            ""style"": ""{style}"",
            ""explode"": true,
            ""type"": ""array"",
            ""items"": {{
              ""type"": ""integer"",
              ""format"": ""int32""
            }}
          }}
        ],
        ""responses"": {{
          ""200"": {{
            ""description"": ""A paged array of settings""
          }}
        }}
      }}
    }}
  }}
}}
";



            var document = OpenApiDocument.FromJsonAsync(spec).Result;

            //// Act
            var generator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings());
            var code = generator.GenerateFile();

            //// Assert

            var expected = "foreach (var item_ in paging) { urlBuilder_.Append(System.Uri.EscapeDataString(\"paging\") + \"=\").Append((item_ == null) ? \"\" : System.Uri.EscapeDataString(ConvertToString(item_, System.Globalization.CultureInfo.InvariantCulture))).Append(\"&\"); }";
            
            AssertCodeContains(expected, code);
        }

        private static void AssertCodeContains(string expected, string actual)
        {
            var reader = new StringReader(expected);
            
            const string regexLinePrefix = @"^\s+";
            const string regexLineSuffix = @"\r?\n";

            StringBuilder regexPattern = new StringBuilder();
            while (reader.ReadLine() is string line) 
                regexPattern.AppendLine(regexLinePrefix + Regex.Escape(line) + regexLineSuffix);

            Regex regex = new Regex(regexPattern.ToString(), RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

            Assert.True(regex.IsMatch(actual), $"Unable to find:\r\n{expected}\r\n in:\r\n{actual}");
        }
    }
}