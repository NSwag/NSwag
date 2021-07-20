//-----------------------------------------------------------------------
// <copyright file="SwaggerDocument.Serialization.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NJsonSchema;
using NJsonSchema.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace NSwag
{
    public partial class OpenApiDocument
    {
        private static Lazy<PropertyRenameAndIgnoreSerializerContractResolver> Swagger2ContractResolver =
            new Lazy<PropertyRenameAndIgnoreSerializerContractResolver>(() => CreateJsonSerializerContractResolver(SchemaType.Swagger2));

        private static Lazy<PropertyRenameAndIgnoreSerializerContractResolver> OpenApi3ContractResolver =
            new Lazy<PropertyRenameAndIgnoreSerializerContractResolver>(() => CreateJsonSerializerContractResolver(SchemaType.OpenApi3));

        /// <summary>Creates the serializer contract resolver based on the <see cref="NJsonSchema.SchemaType"/>.</summary>
        /// <param name="schemaType">The schema type.</param>
        /// <returns>The settings.</returns>
        public static PropertyRenameAndIgnoreSerializerContractResolver GetJsonSerializerContractResolver(SchemaType schemaType)
        {
            if (schemaType == SchemaType.Swagger2)
            {
                return Swagger2ContractResolver.Value;
            }
            else if (schemaType == SchemaType.OpenApi3)
            {
                return OpenApi3ContractResolver.Value;
            }

            throw new ArgumentException("The given schema type is not supported.");
        }

        private static PropertyRenameAndIgnoreSerializerContractResolver CreateJsonSerializerContractResolver(SchemaType schemaType)
        {
            var resolver = JsonSchema.CreateJsonSerializerContractResolver(schemaType);

            if (schemaType == SchemaType.Swagger2)
            {
                resolver.IgnoreProperty(typeof(OpenApiDocument), "openapi");
                resolver.IgnoreProperty(typeof(OpenApiDocument), "servers");
                resolver.IgnoreProperty(typeof(OpenApiParameter), "title");

                // TODO: Use rename for not mapped properties!
                resolver.IgnoreProperty(typeof(OpenApiPathItem), "summary");
                resolver.IgnoreProperty(typeof(OpenApiPathItem), "description");
                resolver.IgnoreProperty(typeof(OpenApiPathItem), "servers");

                resolver.IgnoreProperty(typeof(OpenApiOperation), "callbacks");
                resolver.IgnoreProperty(typeof(OpenApiOperation), "servers");
                resolver.IgnoreProperty(typeof(OpenApiOperation), "requestBody");

                resolver.IgnoreProperty(typeof(OpenApiDocument), "components");
                resolver.IgnoreProperty(typeof(OpenApiParameter), "examples");
                resolver.IgnoreProperty(typeof(OpenApiParameter), "x-position");

                resolver.IgnoreProperty(typeof(OpenApiResponse), "content");
                resolver.IgnoreProperty(typeof(OpenApiResponse), "links");

                resolver.IgnoreProperty(typeof(OpenApiSecurityScheme), "scheme");
                resolver.IgnoreProperty(typeof(OpenApiSecurityScheme), "bearerFormat");
                resolver.IgnoreProperty(typeof(OpenApiSecurityScheme), "openIdConnectUrl");
                resolver.IgnoreProperty(typeof(OpenApiSecurityScheme), "flows");
            }
            else if (schemaType == SchemaType.OpenApi3)
            {
                resolver.IgnoreProperty(typeof(OpenApiDocument), "swagger");

                resolver.IgnoreProperty(typeof(OpenApiDocument), "host");
                resolver.IgnoreProperty(typeof(OpenApiDocument), "basePath");
                resolver.IgnoreProperty(typeof(OpenApiDocument), "schemes");

                resolver.IgnoreProperty(typeof(OpenApiDocument), "consumes");
                resolver.IgnoreProperty(typeof(OpenApiDocument), "produces");

                resolver.IgnoreProperty(typeof(OpenApiOperation), "schemes");
                resolver.IgnoreProperty(typeof(OpenApiOperation), "consumes");
                resolver.IgnoreProperty(typeof(OpenApiOperation), "produces");

                //resolver.IgnoreProperty(typeof(SwaggerParameter), "x-nullable");

                //resolver.IgnoreProperty(typeof(SwaggerResponse), "consumes"); => TODO map to response.content
                //resolver.IgnoreProperty(typeof(SwaggerResponse), "produces");

                resolver.IgnoreProperty(typeof(OpenApiDocument), "definitions");
                resolver.IgnoreProperty(typeof(OpenApiDocument), "parameters");
                resolver.IgnoreProperty(typeof(OpenApiDocument), "responses");
                resolver.IgnoreProperty(typeof(OpenApiDocument), "securityDefinitions");

                resolver.IgnoreProperty(typeof(OpenApiResponse), "schema");
                resolver.IgnoreProperty(typeof(OpenApiResponse), "examples");
                resolver.IgnoreProperty(typeof(OpenApiResponse), "x-nullable");

                resolver.IgnoreProperty(typeof(OpenApiSecurityScheme), "flow");
                resolver.IgnoreProperty(typeof(OpenApiSecurityScheme), "authorizationUrl");
                resolver.IgnoreProperty(typeof(OpenApiSecurityScheme), "tokenUrl");
                resolver.IgnoreProperty(typeof(OpenApiSecurityScheme), "scopes");
            }
            else
            {
                throw new ArgumentException("The given schema type is not supported.");
            }

            return resolver;
        }

        private ObservableCollection<OpenApiSchema> _schemes = new ObservableCollection<OpenApiSchema>();

        /// <summary>Gets or sets the host (name or ip) serving the API (Swagger only).</summary>
        [JsonProperty(PropertyName = "host", Order = 5, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Host
        {
            get { return Servers?.FirstOrDefault()?.Url?.Replace("http://", "").Replace("https://", "").Split('/')[0]; }
            set { UpdateServers(Schemes, value, BasePath); }
        }

        /// <summary>Gets or sets the base path on which the API is served, which is relative to the <see cref="Host"/>.</summary>
        [JsonProperty(PropertyName = "basePath", Order = 6, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string BasePath
        {
            get
            {
                var segments = Servers?.FirstOrDefault()?.Url?.Replace("http://", "").Replace("https://", "").Split('/').Skip(1).ToArray();
                return segments != null && segments.Length > 0 ? "/" + string.Join("/", segments) : null;
            }
            set { UpdateServers(Schemes, Host, value); }
        }

        /// <summary>Gets or sets the schemes.</summary>
        [JsonProperty(PropertyName = "schemes", Order = 7, DefaultValueHandling = DefaultValueHandling.Ignore, ItemConverterType = typeof(StringEnumConverter))]
        public ICollection<OpenApiSchema> Schemes
        {
            get
            {
                if (_schemes != null)
                {
                    _schemes.CollectionChanged -= OnSchemesChanged;
                }

                _schemes = new ObservableCollection<OpenApiSchema>(Servers?
                    .Where(s => s.Url.Contains("://"))
                    .Select(s => s.Url.StartsWith("http://") ? OpenApiSchema.Http : OpenApiSchema.Https)
                    .Distinct() ?? new List<OpenApiSchema>());

                _schemes.CollectionChanged += OnSchemesChanged;

                return _schemes;
            }
            set { UpdateServers(value, Host, BasePath); }
        }

        private void OnSchemesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateServers((ICollection<OpenApiSchema>)sender, Host, BasePath);
        }

        private void UpdateServers(ICollection<OpenApiSchema> schemes, string host, string basePath)
        {
            if ((schemes == null || schemes.Count == 0) && (!string.IsNullOrEmpty(host) || !string.IsNullOrEmpty(basePath)))
            {
                Servers = new List<OpenApiServer>
                {
                    new OpenApiServer
                    {
                        Url = host + basePath
                    }
                };
            }
            else
            {
                Servers = schemes?.Select(s => new OpenApiServer
                {
                    Url = s.ToString().ToLowerInvariant() + "://" + host + basePath
                }).ToList() ?? new List<OpenApiServer>();
            }
        }

        /// <summary>Gets or sets a list of MIME types the operation can consume.</summary>
        [JsonProperty(PropertyName = "consumes", Order = 8, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ICollection<string> Consumes { get; set; } = new List<string>();

        /// <summary>Gets or sets a list of MIME types the operation can produce.</summary>
        [JsonProperty(PropertyName = "produces", Order = 9, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ICollection<string> Produces { get; set; } = new List<string>();

        /// <summary>Gets or sets the types (Swagger only).</summary>
        [JsonProperty(PropertyName = "definitions", Order = 13, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IDictionary<string, JsonSchema> Definitions => Components.Schemas;

        /// <summary>Gets or sets a list of type mappings to be used for post processing.</summary>
        [JsonIgnore]
        public List<TypeDefinitionTracker> DefinitionsMap { get; set; } = new List<TypeDefinitionTracker>();

        /// <summary>Gets or sets the parameters which can be used for all operations (Swagger only).</summary>
        [JsonProperty(PropertyName = "parameters", Order = 14, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IDictionary<string, OpenApiParameter> Parameters => Components.Parameters;

        /// <summary>Gets or sets the responses which can be used for all operations (Swagger only).</summary>
        [JsonProperty(PropertyName = "responses", Order = 15, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IDictionary<string, OpenApiResponse> Responses => Components.Responses;

        /// <summary>Gets or sets the security definitions (Swagger only).</summary>
        [JsonProperty(PropertyName = "securityDefinitions", Order = 16, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IDictionary<string, OpenApiSecurityScheme> SecurityDefinitions => Components.SecuritySchemes;
    }
}
