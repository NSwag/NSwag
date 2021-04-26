using System.Linq;
using NJsonSchema;
using Xunit;

namespace NSwag.Core.Tests.Serialization
{
    public class ServersSerializationTests
    {
        [Fact]
        public void When_document_is_empty_then_serialized_correctly_in_Swagger()
        {
            // Arrange
            var document = new OpenApiDocument();

            // Act
            var json = document.ToJson(SchemaType.Swagger2);

            // Assert
            Assert.Equal(
@"{
  ""swagger"": ""2.0"",
  ""info"": {
    ""title"": ""Swagger specification"",
    ""version"": ""1.0.0""
  }
}".Replace("\r", ""), json.Replace("\r", ""));
        }

        [Fact]
        public void When_schema_is_added_to_definitions_then_it_is_serialized_correctly_in_Swagger()
        {
            // Arrange
            var document = CreateDocument();

            // Act
            var json = document.ToJson(SchemaType.Swagger2);

            // Assert
            Assert.Equal("rsuter.com", document.Host);
            Assert.Equal("/myapi", document.BasePath);
            Assert.Equal(2, document.Schemes.Count);
            Assert.Equal(OpenApiSchema.Http, document.Schemes.First());
            Assert.Equal(OpenApiSchema.Https, document.Schemes.Last());

            Assert.Contains(@"""basePath""", json);
        }

        [Fact]
        public void When_schema_is_added_to_definitions_then_it_is_serialized_correctly_in_OpenApi()
        {
            // Arrange
            var document = CreateDocument();

            // Act
            var json = document.ToJson(SchemaType.OpenApi3);

            // Assert
            Assert.Equal(2, document.Servers.Count);
            Assert.Contains(@"""servers""", json);
        }

        [Fact]
        public void When_server_is_set_then_it_is_correctly_converted_to_Swagger()
        {
            // Arrange
            var document = new OpenApiDocument
            {
                Servers =
                {
                    new OpenApiServer
                    {
                        Url = "http://localhost:12354/myapi"
                    }
                }
            };

            // Act
            var json = document.ToJson(SchemaType.Swagger2);

            // Assert
            Assert.Equal("localhost:12354", document.Host);
            Assert.Equal("/myapi", document.BasePath);
            Assert.Equal(1, document.Schemes.Count);
            Assert.Equal(OpenApiSchema.Http, document.Schemes.First());
        }

        [Fact]
        public void When_host_basePath_and_schemeas_are_set_then_it_is_correctly_converted_to_OpenApi()
        {
            // Arrange
            var document = new OpenApiDocument
            {
                Host = "localhost:12354",
                BasePath = "/myapi",
                Schemes = { OpenApiSchema.Http }
            };

            // Act
            var json = document.ToJson(SchemaType.Swagger2);

            // Assert
            Assert.Equal("localhost:12354", document.Host);
            Assert.Equal("/myapi", document.BasePath);
            Assert.Equal(1, document.Schemes.Count);
            Assert.Equal(OpenApiSchema.Http, document.Schemes.First());
        }

        [Fact]
        public void When_scheme_without_host_is_added_then_servers_are_not_cleared()
        {
            // Arrange
            var document = new OpenApiDocument
            {
                BasePath = "/myapi",
                Schemes = { OpenApiSchema.Http }
            };

            // Act
            document.Schemes.Add(OpenApiSchema.Https);
            document.Host = "localhost:12354";

            var json = document.ToJson(SchemaType.Swagger2);

            // Assert
            Assert.Equal(2, document.Servers.Count);
        }

        [Fact]
        public void When_host_is_removed_then_base_url_is_also_empty()
        {
            // Arrange
            var document = new OpenApiDocument
            {
                Host = "localhost:12354",
                BasePath = "/myapi",
                Schemes = { OpenApiSchema.Http }
            };

            // Act
            document.Host = string.Empty;
            var json = document.ToJson(SchemaType.Swagger2);

            // Assert
            Assert.True(string.IsNullOrEmpty(document.BaseUrl));
        }

        private static OpenApiDocument CreateDocument()
        {
            var document = new OpenApiDocument
            {
                Servers =
                {
                    new OpenApiServer
                    {
                        Url = "http://rsuter.com/myapi"
                    }
                }
            };

            document.Schemes.Add(OpenApiSchema.Https);

            return document;
        }
    }
}