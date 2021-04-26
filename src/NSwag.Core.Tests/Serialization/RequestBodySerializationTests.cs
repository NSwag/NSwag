using System.Linq;
using System.Threading.Tasks;
using NJsonSchema;
using Xunit;

namespace NSwag.Core.Tests.Serialization
{
    public class RequestBodySerializationTests
    {
        [Fact]
        public async Task When_request_body_is_added_then_serialized_correctly_in_Swagger()
        {
            // Arrange
            var document = CreateDocument();

            // Act
            var json = document.ToJson(SchemaType.Swagger2);
            document = await OpenApiDocument.FromJsonAsync(json);

            // Assert
            var requestBody = document.Paths["/baz"][OpenApiOperationMethod.Get].RequestBody;
            Assert.Equal("foo", requestBody.Name);
        }

        [Fact]
        public async Task When_request_body_is_added_then_serialized_correctly_in_OpenApi()
        {
            // Arrange
            var document = CreateDocument();

            // Act
            var json = document.ToJson(SchemaType.OpenApi3);
            document = await OpenApiDocument.FromJsonAsync(json);

            // Assert
            var requestBody = document.Paths["/baz"][OpenApiOperationMethod.Get].RequestBody;
            Assert.Equal("foo", requestBody.Name);
        }

        [Fact]
        public async Task When_body_parameter_is_changed_then_request_body_IsRequired_is_updated()
        {
            // Arrange
            var document = CreateDocument();

            // Act
            var json = document.ToJson(SchemaType.OpenApi3);
            document = await OpenApiDocument.FromJsonAsync(json);

            var parameter = document.Paths["/baz"][OpenApiOperationMethod.Get].Parameters
                .Single(p => p.Kind == OpenApiParameterKind.Body);

            parameter.IsRequired = true;

            // Assert
            var requestBody = document.Paths["/baz"][OpenApiOperationMethod.Get].RequestBody;

            Assert.True(requestBody.IsRequired);
            Assert.True(parameter.IsRequired);
        }

        [Fact]
        public async Task When_body_parameter_is_changed_then_request_body_Name_is_updated()
        {
            // Arrange
            var document = CreateDocument();

            // Act
            var json = document.ToJson(SchemaType.OpenApi3);
            document = await OpenApiDocument.FromJsonAsync(json);

            var parameter = document.Paths["/baz"][OpenApiOperationMethod.Get].Parameters
                .Single(p => p.Kind == OpenApiParameterKind.Body);

            parameter.Name = parameter.Name + "123";

            // Assert
            var requestBody = document.Paths["/baz"][OpenApiOperationMethod.Get].RequestBody;

            Assert.Equal("foo123", requestBody.Name);
        }

        [Fact]
        public async Task When_body_parameter_is_changed_then_request_body_Schema_is_updated()
        {
            // Arrange
            var document = CreateDocument();

            // Act
            var json = document.ToJson(SchemaType.OpenApi3);
            document = await OpenApiDocument.FromJsonAsync(json);

            var parameter = document.Paths["/baz"][OpenApiOperationMethod.Get].Parameters
                .Single(p => p.Kind == OpenApiParameterKind.Body);

            parameter.Schema = new JsonSchema { Title = "blub" };

            // Assert
            var requestBody = document.Paths["/baz"][OpenApiOperationMethod.Get].RequestBody;

            Assert.Equal("blub", requestBody.Content["application/json"].Schema.Title);
        }

        [Fact]
        public async Task When_body_parameter_is_changed_then_request_body_Description_is_updated()
        {
            // Arrange
            var document = CreateDocument();

            // Act
            var json = document.ToJson(SchemaType.OpenApi3);
            document = await OpenApiDocument.FromJsonAsync(json);

            var parameter = document.Paths["/baz"][OpenApiOperationMethod.Get].Parameters
                .Single(p => p.Kind == OpenApiParameterKind.Body);

            parameter.Description = parameter.Description + "123";

            // Assert
            var requestBody = document.Paths["/baz"][OpenApiOperationMethod.Get].RequestBody;

            Assert.Equal("bar123", requestBody.Description);
        }

        [Fact]
        public async Task When_request_body_is_changed_then_body_parameter_Name_is_updated()
        {
            // Arrange
            var document = CreateDocument();

            // Act
            var json = document.ToJson(SchemaType.OpenApi3);
            document = await OpenApiDocument.FromJsonAsync(json);

            var requestBody = document.Paths["/baz"][OpenApiOperationMethod.Get].RequestBody;

            requestBody.Name = requestBody.Name + "123";

            // Assert
            var parameter = document.Paths["/baz"][OpenApiOperationMethod.Get].Parameters
                .Single(p => p.Kind == OpenApiParameterKind.Body);

            Assert.Equal("foo123", parameter.Name);
        }

        [Fact]
        public async Task When_request_body_is_changed_then_body_parameter_IsRequired_is_updated()
        {
            // Arrange
            var document = CreateDocument();

            // Act
            var json = document.ToJson(SchemaType.OpenApi3);
            document = await OpenApiDocument.FromJsonAsync(json);

            var requestBody = document.Paths["/baz"][OpenApiOperationMethod.Get].RequestBody;

            requestBody.IsRequired = true;

            // Assert
            var parameter = document.Paths["/baz"][OpenApiOperationMethod.Get].Parameters
                .Single(p => p.Kind == OpenApiParameterKind.Body);

            Assert.True(parameter.IsRequired);
            Assert.True(requestBody.IsRequired);
        }

        [Fact]
        public async Task When_request_body_is_changed_then_body_parameter_Content_is_updated()
        {
            // Arrange
            var document = CreateDocument();

            // Act
            var json = document.ToJson(SchemaType.OpenApi3);
            document = await OpenApiDocument.FromJsonAsync(json);

            var requestBody = document.Paths["/baz"][OpenApiOperationMethod.Get].RequestBody;
            requestBody.Content["application/json"] = new OpenApiMediaType
            {
                Schema = new JsonSchema { Title = "blub" }
            };

            // Assert
            var parameter = document.Paths["/baz"][OpenApiOperationMethod.Get].Parameters
                .Single(p => p.Kind == OpenApiParameterKind.Body);

            Assert.Equal("blub", parameter.Schema.Title);
        }

        [Fact]
        public async Task When_request_body_is_changed_then_body_parameter_Description_is_updated()
        {
            // Arrange
            var document = CreateDocument();

            // Act
            var json = document.ToJson(SchemaType.OpenApi3);
            document = await OpenApiDocument.FromJsonAsync(json);

            var requestBody = document.Paths["/baz"][OpenApiOperationMethod.Get].RequestBody;
            requestBody.Description = requestBody.Description + "123";

            // Assert
            var parameter = document.Paths["/baz"][OpenApiOperationMethod.Get].Parameters
                .Single(p => p.Kind == OpenApiParameterKind.Body);

            Assert.Equal("bar123", parameter.Description);
        }

        private static OpenApiDocument CreateDocument()
        {
            var schema = new JsonSchema
            {
                Type = JsonObjectType.String
            };

            var document = new OpenApiDocument
            {
                Paths =
                {
                    {
                        "/baz",
                        new OpenApiPathItem
                        {
                            {
                                OpenApiOperationMethod.Get,
                                new OpenApiOperation
                                {
                                    RequestBody = new OpenApiRequestBody
                                    {
                                        Name = "foo",
                                        Description = "bar",
                                        Content =
                                        {
                                            {
                                                "application/json",
                                                new OpenApiMediaType
                                                {
                                                    Schema = new JsonSchema { Reference = schema }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                Definitions =
                {
                    {
                        "Abc",
                        schema
                    }
                }
            };

            return document;
        }
    }
}