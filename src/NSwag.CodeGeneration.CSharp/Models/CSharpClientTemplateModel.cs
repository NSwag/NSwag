//-----------------------------------------------------------------------
// <copyright file="CSharpClientTemplateModel.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;

namespace NSwag.CodeGeneration.CSharp.Models
{
    /// <summary>The CSharp client template model.</summary>
    public class CSharpClientTemplateModel : CSharpTemplateModelBase
    {
        private readonly OpenApiDocument _document;
        private readonly JsonSchema _exceptionSchema;
        private readonly CSharpClientGeneratorSettings _settings;

        /// <summary>Initializes a new instance of the <see cref="CSharpClientTemplateModel" /> class.</summary>
        /// <param name="controllerName">Name of the controller.</param>
        /// <param name="controllerClassName">The class name of the controller.</param>
        /// <param name="operations">The operations.</param>
        /// <param name="exceptionSchema">The exception schema.</param>
        /// <param name="document">The Swagger document.</param>
        /// <param name="settings">The settings.</param>
        public CSharpClientTemplateModel(
            string controllerName,
            string controllerClassName,
            IEnumerable<CSharpOperationModel> operations,
            JsonSchema exceptionSchema,
            OpenApiDocument document,
            CSharpClientGeneratorSettings settings)
            : base(controllerName, settings)
        {
            _document = document;
            _exceptionSchema = exceptionSchema;
            _settings = settings;

            Class = controllerClassName;
            Operations = operations;

            BaseClass = _settings.ClientBaseClass?.Replace("{controller}", controllerName);
            ExceptionClass = _settings.ExceptionClass.Replace("{controller}", controllerName);
        }

        /// <summary>Gets the class name.</summary>
        public string Class { get; }

        /// <summary>Gets a value indicating whether the client has a base class.</summary>
        public bool HasBaseClass => !string.IsNullOrEmpty(BaseClass);

        /// <summary>Gets the base class name.</summary>
        public string BaseClass { get; }

        /// <summary>Gets a value indicating whether the client has configuration class.</summary>
        public bool HasConfigurationClass => !string.IsNullOrEmpty(_settings.ConfigurationClass);

        /// <summary>Gets the configuration class name.</summary>
        public string ConfigurationClass => _settings.ConfigurationClass;

        /// <summary>Gets a value indicating whether the client has a base type.</summary>
        public bool HasBaseType => _settings.GenerateClientInterfaces || HasBaseClass;

        /// <summary>Gets or sets a value indicating whether an HttpClient instance is injected.</summary>
        public bool InjectHttpClient => _settings.InjectHttpClient;

        /// <summary>Gets or sets a value indicating whether to dispose the HttpClient (injected HttpClient is never disposed, default: true).</summary>
        public bool DisposeHttpClient => _settings.DisposeHttpClient;

        /// <summary>Gets a value indicating whether to use a HTTP client creation method.</summary>
        public bool UseHttpClientCreationMethod => _settings.UseHttpClientCreationMethod;

        /// <summary>Gets the type of the HttpClient that will be used in the calls from a client to a service.</summary>
        public string HttpClientType => _settings.HttpClientType;

        /// <summary>Gets a value indicating whether to use a HTTP request message creation method.</summary>
        public bool UseHttpRequestMessageCreationMethod => _settings.UseHttpRequestMessageCreationMethod;

        /// <summary>Gets a value indicating whether to generate client interfaces.</summary>
        public bool GenerateClientInterfaces => _settings.GenerateClientInterfaces;

        /// <summary>Gets client base interface.</summary>
        public string ClientBaseInterface => _settings.ClientBaseInterface;

        /// <summary>Gets a value indicating whether client interface has a base interface.</summary>
        public bool HasClientBaseInterface => !string.IsNullOrEmpty(ClientBaseInterface);

        /// <summary>Gets a value indicating whether the document has a BaseUrl specified.</summary>
        public bool HasBaseUrl => !string.IsNullOrEmpty(BaseUrl);

        /// <summary>Gets the service base URL.</summary>
        public string BaseUrl => _document.BaseUrl;

        /// <summary>Gets a value indicating whether the client has operations.</summary>
        public bool HasOperations => Operations.Any();

        /// <summary>Gets the exception class name.</summary>
        public string ExceptionClass { get; }

        /// <summary>Gets a value indicating whether to generate optional parameters.</summary>
        public bool GenerateOptionalParameters => _settings.GenerateOptionalParameters;

        /// <summary>Gets or sets a value indicating to use default value for optional parameter if present</summary>
        public bool UseDefaultValueOptionalParameter => _settings.UseDefaultValueOptionalParameter;

        /// <summary>Gets or sets a value indicating whether to use and expose the base URL (default: true).</summary>
        public bool UseBaseUrl => _settings.UseBaseUrl;

        /// <summary>Gets or sets a value indicating whether to generate the BaseUrl property, must be defined on the base class otherwise (default: true).</summary>
        public bool GenerateBaseUrlProperty => _settings.GenerateBaseUrlProperty;

        /// <summary>Gets or sets a value indicating whether to generate synchronous methods (not recommended, default: false).</summary>
        public bool GenerateSyncMethods => _settings.GenerateSyncMethods;

        /// <summary>Gets or sets the client class access modifier.</summary>
        public string ClientClassAccessModifier => _settings.ClientClassAccessModifier;

        /// <summary>Gets the operations.</summary>
        public IEnumerable<CSharpOperationModel> Operations { get; }

        /// <summary>Gets the operations of the interface.</summary>
        public IEnumerable<CSharpOperationModel> InterfaceOperations => Operations.Where(o => o.IsInterfaceMethod);

        /// <summary>Gets or sets a value indicating whether DTO exceptions are wrapped in a SwaggerException instance.</summary>
        public bool WrapDtoExceptions => _settings.WrapDtoExceptions;

        /// <summary>Gets or sets the format for DateTime type method parameters.</summary>
        public string ParameterDateTimeFormat => _settings.ParameterDateTimeFormat;

        /// <summary>Gets or sets the format for Date type method parameters.</summary>
        public string ParameterDateFormat => _settings.ParameterDateFormat;

        /// <summary>Gets or sets a value indicating whether to expose the JsonSerializerSettings property.</summary>
        public bool ExposeJsonSerializerSettings => _settings.ExposeJsonSerializerSettings;

        /// <summary>Gets or sets a value indicating whether to generate the UpdateJsonSerializerSettings method.</summary>
        public bool GenerateUpdateJsonSerializerSettingsMethod => _settings.GenerateUpdateJsonSerializerSettingsMethod;

        /// <summary>Gets or sets a value indicating whether to generate different request and response serialization settings (default: false).</summary>
        public bool UseRequestAndResponseSerializationSettings => _settings.UseRequestAndResponseSerializationSettings;

        /// <summary>Gets or sets a value indicating whether to serialize the type information in a $type property (not recommended, also sets TypeNameHandling = Auto).</summary>
        public bool SerializeTypeInformation => _settings.SerializeTypeInformation;

        /// <summary>Gets or sets the null value used for query parameters which are null.</summary>
        public string QueryNullValue => _settings.QueryNullValue;

        /// <summary>
        /// Gets or sets a value indicating whether to create PrepareRequest and ProcessResponse as async methods, or as partial synchronous methods.
        /// If value is set to true, PrepareRequestAsync and ProcessResponseAsync methods must be implemented as part of the client base class (if it has one) or as part of the partial client class.
        /// If value is set to false, PrepareRequest and ProcessResponse methods will be partial methods, and implement them is optional.
        /// </summary>
        public bool GeneratePrepareRequestAndProcessResponseAsAsyncMethods => _settings.GeneratePrepareRequestAndProcessResponseAsAsyncMethods;

        /// <summary>Gets the JSON serializer parameter code.</summary>
        public string JsonSerializerParameterCode
        {
            get
            {
                var parameterCode = CSharpJsonSerializerGenerator.GenerateJsonSerializerParameterCode(
                    _settings.CSharpGeneratorSettings, RequiresJsonExceptionConverter ? new[] { "JsonExceptionConverter" } : null);

                if (!parameterCode.Contains("new Newtonsoft.Json.JsonSerializerSettings"))
                {
                    parameterCode = _settings.CSharpGeneratorSettings.JsonLibrary == CSharpJsonLibrary.NewtonsoftJson ?
                        "new Newtonsoft.Json.JsonSerializerSettings { Converters = " + parameterCode + " }" :
                        parameterCode;
                }

                return parameterCode;
            }
        }

        /// <summary>Gets the JSON converters array code.</summary>
        public string JsonConvertersArrayCode
        {
            get
            {
                return CSharpJsonSerializerGenerator.GenerateJsonConvertersArrayCode(
                    _settings.CSharpGeneratorSettings, RequiresJsonExceptionConverter ? new[] { "JsonExceptionConverter" } : null);
            }
        }

        /// <summary>Gets the Title.</summary>
        public string Title => _document.Info.Title;

        /// <summary>Gets the Description.</summary>
        public string Description => _document.Info.Description;

        /// <summary>Gets the API version.</summary>
        public string Version => _document.Info.Version;

        /// <summary>Gets the extension data.</summary>
        public IDictionary<string, object> ExtensionData => _document.ExtensionData;

        private bool RequiresJsonExceptionConverter =>
            _settings.CSharpGeneratorSettings.JsonLibrary == CSharpJsonLibrary.NewtonsoftJson &&
            _settings.CSharpGeneratorSettings.ExcludedTypeNames?.Contains("JsonExceptionConverter") != true &&
            _document.Operations.Any(o => o.Operation.ActualResponses.Any(r => r.Value.Schema?.InheritsSchema(_exceptionSchema) == true));
    }
}