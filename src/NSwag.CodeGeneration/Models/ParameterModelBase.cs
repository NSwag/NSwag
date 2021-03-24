//-----------------------------------------------------------------------
// <copyright file="ParameterModelBase.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using NJsonSchema;
using NJsonSchema.CodeGeneration;
using System.Collections.Generic;
using System.Linq;

namespace NSwag.CodeGeneration.Models
{
    /// <summary>The parameter template model.</summary>
    public abstract class ParameterModelBase
    {
        private readonly OpenApiParameter _parameter;
        private readonly IList<OpenApiParameter> _allParameters;
        private readonly CodeGeneratorSettingsBase _settings;
        private readonly IClientGenerator _generator;
        private readonly TypeResolverBase _typeResolver;
        private readonly IEnumerable<PropertyModel> _properties;

        /// <summary>Initializes a new instance of the <see cref="ParameterModelBase" /> class.</summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="typeName">The type name.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="allParameters">All parameters.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="generator">The client generator base.</param>
        /// <param name="typeResolver">The type resolver.</param>
        protected ParameterModelBase(string parameterName, string variableName, string typeName,
            OpenApiParameter parameter, IList<OpenApiParameter> allParameters, CodeGeneratorSettingsBase settings,
            IClientGenerator generator, TypeResolverBase typeResolver)
        {
            _allParameters = allParameters;
            _parameter = parameter;
            _settings = settings;
            _generator = generator;
            _typeResolver = typeResolver;

            Type = typeName;
            Name = parameterName;
            VariableName = variableName;

            var propertyNameGenerator = settings?.PropertyNameGenerator ?? throw new InvalidOperationException("PropertyNameGenerator not set.");

            _properties = _parameter.ActualSchema.ActualProperties
                .Select(p => new PropertyModel(p.Key, p.Value, propertyNameGenerator.Generate(p.Value)))
                .ToList();
        }

        /// <summary>Gets the type of the parameter.</summary>
        public string Type { get; }

        /// <summary>Gets the name.</summary>
        public string Name { get; }

        /// <summary>Gets the variable name in (usually lowercase).</summary>
        public string VariableName { get; }

        /// <summary>Gets a value indicating whether a default value is available.</summary>
        public bool HasDefault => Default != null;

        /// <summary>The default value for the variable.</summary>
        public string Default
        {
            get
            {
                if (_settings.SchemaType == SchemaType.Swagger2)
                {
                    return !_parameter.IsRequired && _parameter.Default != null ?
                        _settings.ValueGenerator?.GetDefaultValue(_parameter, false, _parameter.ActualTypeSchema.Id, null, true, _typeResolver) :
                        null;
                }
                else
                {
                    return !_parameter.IsRequired && _parameter.ActualSchema.Default != null ?
                        _settings.ValueGenerator?.GetDefaultValue(_parameter.ActualSchema, false, _parameter.ActualTypeSchema.Id, null, true, _typeResolver) :
                        null;
                }
            }
        }

        /// <summary>Gets the parameter kind.</summary>
        public OpenApiParameterKind Kind => _parameter.Kind;

        /// <summary>Gets the parameter style.</summary>
        public OpenApiParameterStyle Style => _parameter.ActualStyle;

        /// <summary>Gets the the value indicating if the parameter values should be exploded when included in the query string.</summary>
        public bool Explode => _parameter.ActualExplode;

        /// <summary>Gets a value indicating whether the parameter is a deep object (OpenAPI 3).</summary>
        public bool IsDeepObject => _parameter.ActualStyle == OpenApiParameterStyle.DeepObject;

        /// <summary>Gets a value indicating whether the parameter has form style.</summary>
        public bool IsForm => _parameter.ActualStyle == OpenApiParameterStyle.Form;

        /// <summary>
        /// Gets a value indicating whether this array parameter has space-delimited style (OpenAPI 3).
        /// </summary>
        public bool IsSpaceDelimited => this._parameter.ActualStyle == OpenApiParameterStyle.SpaceDelimeted;

        /// <summary>
        /// Gets a value indicating whether this array parameter has pipe-delimited style (OpenAPI 3).
        /// </summary>
        public bool IsPipeDelimited => this._parameter.ActualStyle == OpenApiParameterStyle.PipeDelimited;

        /// <summary>Gets the contained value property names (OpenAPI 3).</summary>
        public IEnumerable<PropertyModel> PropertyNames
        {
            get
            {
                return _properties.Where(p => !p.IsCollection);
            }
        }

        /// <summary>Gets the contained collection property names (OpenAPI 3).</summary>
        public IEnumerable<PropertyModel> CollectionPropertyNames
        {
            get
            {
                return _properties.Where(p => p.IsCollection);
            }
        }

        /// <summary>Gets a value indicating whether the parameter has a description.</summary>
        public bool HasDescription => !string.IsNullOrEmpty(Description);

        /// <summary>Gets the parameter description.</summary>
        public string Description => ConversionUtilities.TrimWhiteSpaces(_parameter.Description);

        /// <summary>Gets the schema.</summary>
        public JsonSchema Schema => _parameter.ActualSchema;

        /// <summary>Gets a value indicating whether the parameter is required.</summary>
        public bool IsRequired => _parameter.IsRequired;

        /// <summary>Gets a value indicating whether the parameter is nullable.</summary>
        public bool IsNullable => _parameter.IsNullable(_settings.SchemaType);

        /// <summary>Gets a value indicating whether the parameter is optional (i.e. not required).</summary>
        public bool IsOptional => _parameter.IsRequired == false;

        /// <summary>Gets a value indicating whether the parameter has a description or is optional.</summary>
        public bool HasDescriptionOrIsOptional => HasDescription || !IsRequired;

        /// <summary>Gets a value indicating whether the parameter is the last parameter of the operation.</summary>
        public bool IsLast => _allParameters.LastOrDefault() == _parameter;

        /// <summary>Gets a value indicating whether this is an XML body parameter.</summary>
        public bool IsXmlBodyParameter => _parameter.IsXmlBodyParameter;

        /// <summary>Gets a value indicating whether this is an binary body parameter.</summary>
        public bool IsBinaryBodyParameter => _parameter.IsBinaryBodyParameter;

        /// <summary>Gets a value indicating whether the parameter is of type date.</summary>
        public bool IsDate =>
            Schema.Format == JsonFormatStrings.Date &&
            _generator.GetTypeName(Schema, IsNullable, null) != "string";

        /// <summary>Gets a value indicating whether the parameter is of type date-time</summary>
        public bool IsDateTime =>
            Schema.Format == JsonFormatStrings.DateTime && _generator.GetTypeName(Schema, IsNullable, null) != "string";

        /// <summary>Gets a value indicating whether the parameter is of type date-time or date</summary>
        public bool IsDateOrDateTime => IsDate || IsDateTime;

        /// <summary>Gets a value indicating whether the parameter is of type array.</summary>
        public bool IsArray => Schema.Type.HasFlag(JsonObjectType.Array) || _parameter.CollectionFormat == OpenApiParameterCollectionFormat.Multi;

        /// <summary>Gets a value indicating whether the parameter is a string array.</summary>
        public bool IsStringArray => IsArray && Schema.Item?.ActualSchema.Type.HasFlag(JsonObjectType.String) == true;

        /// <summary>Gets a value indicating whether this is a file parameter.</summary>
        public bool IsFile => Schema.IsBinary || (IsArray && Schema?.Item?.IsBinary == true);

        /// <summary>Gets a value indicating whether the parameter is a binary body parameter.</summary>
        public bool IsBinaryBody => _parameter.IsBinaryBodyParameter;

        /// <summary>Gets a value indicating whether a binary body parameter allows multiple mime types.</summary>
        public bool HasBinaryBodyWithMultipleMimeTypes => _parameter.HasBinaryBodyWithMultipleMimeTypes;

        /// <summary>Gets a value indicating whether the parameter is of type dictionary.</summary>
        public bool IsDictionary => Schema.IsDictionary;

        /// <summary>Gets a value indicating whether the parameter is of type date-time array.</summary>
        public bool IsDateTimeArray =>
            IsArray &&
            Schema.Item?.ActualSchema.Format == JsonFormatStrings.DateTime &&
            _generator.GetTypeName(Schema.Item.ActualSchema, IsNullable, null) != "string";

        /// <summary>Gets a value indicating whether the parameter is of type date-time or date array.</summary>
        public bool IsDateOrDateTimeArray => IsDateArray || IsDateTimeArray;

        /// <summary>Gets a value indicating whether the parameter is of type date array.</summary>
        public bool IsDateArray =>
            IsArray &&
            Schema.Item?.ActualSchema.Format == JsonFormatStrings.Date &&
            _generator.GetTypeName(Schema.Item.ActualSchema, IsNullable, null) != "string";

        /// <summary>Gets a value indicating whether the parameter is of type object array.</summary>
        public bool IsObjectArray => IsArray &&
            (Schema.Item?.ActualSchema.Type == JsonObjectType.Object ||
             Schema.Item?.ActualSchema.IsAnyType == true);

        /// <summary>Gets a value indicating whether the parameter is of type object</summary>
        public bool IsObject => Schema.ActualSchema.Type == JsonObjectType.Object;

        /// <summary>Gets a value indicating whether the parameter is of type object.</summary>
        public bool IsBody => Kind == OpenApiParameterKind.Body;

        /// <summary>Gets a value indicating whether the parameter is supplied as query parameter.</summary>
        public bool IsQuery => Kind == OpenApiParameterKind.Query;

        /// <summary>Gets a value indicating whether the parameter is supplied through the request headers.</summary>
        public bool IsHeader => Kind == OpenApiParameterKind.Header;

        /// <summary>Gets the operation extension data.</summary>
        public IDictionary<string, object> ExtensionData => _parameter.ExtensionData;
    }
}
