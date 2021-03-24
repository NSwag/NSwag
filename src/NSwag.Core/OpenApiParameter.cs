//-----------------------------------------------------------------------
// <copyright file="SwaggerParameter.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using NJsonSchema;

namespace NSwag
{
    /// <summary>Describes an operation parameter. </summary>
    public class OpenApiParameter : JsonSchema
    {
        private string _name;
        private OpenApiParameterKind _kind;
        private OpenApiParameterStyle _style;
        private bool _isRequired = false;
        private JsonSchema _schema;
        private IDictionary<string, OpenApiExample> _examples;
        private bool? _explode;
        private int? _position;

        private static readonly Regex AppJsonRegex = new Regex(@"application\/(\S+?)?\+?json;?(\S+)?");

        [JsonIgnore]
        internal OpenApiOperation ParentOperation => Parent as OpenApiOperation;

        /// <summary>Gets or sets the name.</summary>
        [JsonProperty(PropertyName = "name", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                ParentOperation?.UpdateRequestBody(this);
            }
        }

        /// <summary>Gets or sets the kind of the parameter.</summary>
        [JsonProperty(PropertyName = "in", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public OpenApiParameterKind Kind
        {
            get => _kind;
            set
            {
                _kind = value;
                ParentOperation?.UpdateRequestBody(this);
            }
        }

        /// <summary>Gets the actual style of the parameter (OpenAPI only).</summary>
        [JsonIgnore]
        public OpenApiParameterStyle ActualStyle
        {
            get
            {
                OpenApiParameterStyle ret;
                if (_style == OpenApiParameterStyle.Undefined)
                {
                    switch (Kind)
                    {
                        case OpenApiParameterKind.Query:
                        case OpenApiParameterKind.Cookie:
                            ret = OpenApiParameterStyle.Form;
                            break;
                        case OpenApiParameterKind.Path:
                        case OpenApiParameterKind.Header:
                            ret = OpenApiParameterStyle.Simple;
                            break;
                        default:
                            ret = OpenApiParameterStyle.Undefined;
                            break;
                    }
                }
                else
                    ret = _style;

                return ret;
            }
        }

        /// <summary>Gets or sets the style of the parameter (OpenAPI only).</summary>
        [JsonProperty(PropertyName = "style", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public OpenApiParameterStyle Style
        {
            get => _style;
            set
            {
                _style = value;
                ParentOperation?.UpdateRequestBody(this);
            }
        }

        /// <summary>Gets the actual explode setting for the parameter (OpenAPI only).</summary>
        [JsonIgnore]
        public bool ActualExplode => _explode ?? ActualStyle == OpenApiParameterStyle.Form;

        /// <summary>Gets or sets the explode setting for the parameter (OpenAPI only).</summary>
        [JsonProperty(PropertyName = "explode", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Explode
        {
            get => _explode ?? ActualExplode;
            set
            {
                _explode = value;
                ParentOperation?.UpdateRequestBody(this);
            }
        }

        /// <summary>Gets or sets a value indicating whether the parameter is required (default: false).</summary>
        [JsonProperty(PropertyName = "required", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool IsRequired
        {
            get => _isRequired;
            set
            {
                _isRequired = value;
                ParentOperation?.UpdateRequestBody(this);
            }
        }

        /// <summary>Gets or sets a value indicating whether passing empty-valued parameters is allowed (default: false).</summary>
        [JsonProperty(PropertyName = "allowEmptyValue", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool AllowEmptyValue { get; set; }

        /// <summary>Gets or sets the description. </summary>
        [Newtonsoft.Json.JsonProperty("description", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public override string Description
        {
            get => base.Description;
            set
            {
                base.Description = value;
                ParentOperation?.UpdateRequestBody(this);
            }
        }

        /// <summary>Gets the actual parameter.</summary>
        [JsonIgnore]
        public OpenApiParameter ActualParameter => Reference is OpenApiParameter ? (OpenApiParameter)Reference : this;

        /// <summary>Gets or sets the format of the array if type array is used.</summary>
        [JsonProperty(PropertyName = "collectionFormat", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public OpenApiParameterCollectionFormat CollectionFormat { get; set; }

        /// <summary>Gets or sets the headers (OpenAPI only).</summary>
        [JsonProperty(PropertyName = "examples", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IDictionary<string, OpenApiExample> Examples
        {
            get => _examples;
            set
            {
                _examples = value;
                ParentOperation?.UpdateRequestBody(this);
            }
        }

        /// <summary>Gets or sets the schema which is only available when <see cref="Kind"/> == body.</summary>
        [JsonProperty(PropertyName = "schema", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public JsonSchema Schema
        {
            get => _schema;
            set
            {
                _schema = value;
                ParentOperation?.UpdateRequestBody(this);
            }
        }

        /// <summary>Gets or sets the custom schema which is used when <see cref="Kind"/> != body.</summary>
        [JsonProperty(PropertyName = "x-schema", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public JsonSchema CustomSchema { get; set; }

        /// <summary>Gets or sets the name.</summary>
        [JsonProperty(PropertyName = "x-position", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int? Position
        {
            get => _position;
            set
            {
                _position = value;
                ParentOperation?.UpdateRequestBody(this);
            }
        }

        /// <summary>Gets the actual schema, either the parameter schema itself (or its reference) or the <see cref="Schema"/> property when <see cref="Kind"/> == body.</summary>
        /// <exception cref="InvalidOperationException" accessor="get">The schema reference path is not resolved.</exception>
        [JsonIgnore]
        public override JsonSchema ActualSchema
        {
            get
            {
                if (Reference is OpenApiParameter parameter)
                {
                    return parameter.ActualSchema;
                }
                else
                {
                    return Schema?.ActualSchema ?? CustomSchema?.ActualSchema ?? base.ActualSchema;
                }
            }
        }

        /// <summary>Gets a value indicating whether the validated data can be null.</summary>
        /// <param name="schemaType">The schema type.</param>
        /// <returns>The result.</returns>
        public override bool IsNullable(SchemaType schemaType)
        {
            if (schemaType == SchemaType.Swagger2)
            {
                if (IsNullableRaw == null)
                {
                    return IsRequired == false;
                }

                return IsNullableRaw.Value;
            }
            else if (schemaType == SchemaType.OpenApi3)
            {
                if (IsNullableRaw.HasValue)
                {
                    return IsNullableRaw.Value;
                }
                else if (Schema != null)
                {
                    return Schema.IsNullable(schemaType);
                }
                else if (CustomSchema != null)
                {
                    return CustomSchema.IsNullable(schemaType);
                }
            }

            return base.IsNullable(schemaType);
        }

        /// <summary>Gets a value indicating whether this is an XML body parameter.</summary>
        [JsonIgnore]
        public bool IsXmlBodyParameter
        {
            get
            {
                if (Kind != OpenApiParameterKind.Body)
                {
                    return false;
                }

                var parent = Parent as OpenApiOperation;
                var consumes = parent?.ActualConsumes?.Any() == true ?
                    parent.ActualConsumes :
                    parent?.RequestBody?.Content.Keys;

                return consumes?.Any() == true &&
                       consumes.Any(p => p.Contains("application/xml")) &&
                       consumes.Any(p => AppJsonRegex.IsMatch(p)) == false;
            }
        }

        /// <summary>Gets a value indicating whether this is a binary body parameter.</summary>
        [JsonIgnore]
        public bool IsBinaryBodyParameter
        {
            get
            {
                if (Kind != OpenApiParameterKind.Body || IsXmlBodyParameter)
                {
                    return false;
                }

                var parent = Parent as OpenApiOperation;
                if (parent?.ActualConsumes?.Any() == true)
                {
                    var consumes = parent.ActualConsumes;
                    return consumes?.Any() == true &&
                           (Schema?.IsBinary != false || 
                            consumes.Contains("multipart/form-data")) &&
                           consumes?.Any(p => p.Contains("*/*")) == false &&
                           consumes.Any(p => AppJsonRegex.IsMatch(p)) == false;
                }
                else
                {
                    var consumes = parent?.RequestBody?.Content;
                    return (consumes?.Any(p => p.Key == "multipart/form-data") == true ||
                            consumes?.Any(p => p.Value.Schema?.IsBinary != false) == true) &&
                           consumes.Any(p => p.Key.Contains("*/*") && p.Value.Schema?.IsBinary != true) == false &&
                           consumes.Any(p => AppJsonRegex.IsMatch(p.Key) && p.Value.Schema?.IsBinary != true) == false;
                }
            }
        }

        /// <summary>Gets a value indicating whether a binary body parameter allows multiple mime types.</summary>
        [JsonIgnore]
        public bool HasBinaryBodyWithMultipleMimeTypes
        {
            get
            {
                if (!IsBinaryBodyParameter)
                {
                    return false;
                }

                var parent = Parent as OpenApiOperation;
                if (parent?.ActualConsumes?.Any() == true)
                {
                    var consumes = parent.ActualConsumes;
                    return consumes?.Any() == true &&
                           (consumes.Count() > 1 ||
                            consumes.Any(p => p.Contains("*")));
                }
                else
                {
                    var consumes = parent?.RequestBody?.Content;
                    return consumes?.Any() == true &&
                           (consumes.Count() > 1 ||
                            consumes.Any(p => p.Key.Contains("*")));
                }
            }
        }
    }
}
