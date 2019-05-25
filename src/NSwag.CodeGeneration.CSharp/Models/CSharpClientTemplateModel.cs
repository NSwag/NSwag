//-----------------------------------------------------------------------
// <copyright file="CSharpClientTemplateModel.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
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
        private readonly SwaggerDocument _document;
        private readonly JsonSchema _exceptionSchema;
        private readonly SwaggerToCSharpClientGeneratorSettings _settings;

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
            SwaggerDocument document,
            SwaggerToCSharpClientGeneratorSettings settings)
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

        /// <summary>Gets or sets a value indicating whether to use and expose the base URL (default: true).</summary>
        public bool UseBaseUrl => _settings.UseBaseUrl;

        /// <summary>Gets or sets a value indicating whether to generate the BaseUrl property, must be defined on the base class otherwise (default: true).</summary>
        public bool GenerateBaseUrlProperty => _settings.GenerateBaseUrlProperty;

        /// <summary>Gets or sets a value indicating whether to generate synchronous methods (not recommended, default: false).</summary>
        public bool GenerateSyncMethods => _settings.GenerateSyncMethods;

        /// <summary>Gets or sets a value indicating whether to generate asynchronous methods without CancellationToken (default: true).</summary>
        public bool InterfaceAsyncMethodsWithoutCancellationToken => _settings.InterfaceCancellationTokenParameter.HasFlag(CancellationTokenParameterOption.No);

        /// <summary>Gets or sets a value indicating whether to generate asynchronous methods with CancellationToken (default: true).</summary>
        public bool InterfaceAsyncMethodsWithCancellationToken => _settings.InterfaceCancellationTokenParameter.HasFlag(CancellationTokenParameterOption.Yes);

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

        /// <summary>Gets or sets a value indicating whether to expose the JsonSerializerSettings property.</summary>
        public bool ExposeJsonSerializerSettings => _settings.ExposeJsonSerializerSettings;

        /// <summary>Gets or sets a value indicating whether to generate the UpdateJsonSerializerSettings method.</summary>
        public bool GenerateUpdateJsonSerializerSettingsMethod => _settings.GenerateUpdateJsonSerializerSettingsMethod;

        /// <summary>Gets or sets a value indicating whether to serialize the type information in a $type property (not recommended, also sets TypeNameHandling = Auto).</summary>
        public bool SerializeTypeInformation => _settings.SerializeTypeInformation;

        /// <summary>Gets or sets the null value used for query parameters which are null.</summary>
        public string QueryNullValue => _settings.QueryNullValue;

        /// <summary>Gets the JSON serializer parameter code.</summary>
        public string JsonSerializerParameterCode
        {
            get
            {
                // TODO: Fix this in NJS (remove ", ", cleanup)
                var parameterCode = CSharpJsonSerializerGenerator.GenerateJsonSerializerParameterCode(
                    _settings.CSharpGeneratorSettings, RequiresJsonExceptionConverter ? new[] { "JsonExceptionConverter" } : null);

                if (string.IsNullOrEmpty(parameterCode))
                {
                    parameterCode = "new Newtonsoft.Json.JsonSerializerSettings()";
                }
                else if(!parameterCode.Contains("new Newtonsoft.Json.JsonSerializerSettings"))
                {
                    parameterCode = "new Newtonsoft.Json.JsonSerializerSettings { Converters = " + parameterCode.Substring(2) + " }";
                }
                else
                {
                    parameterCode = parameterCode.Substring(2);
                }

                return parameterCode;
            }
        }

        private bool RequiresJsonExceptionConverter => _settings.CSharpGeneratorSettings.ExcludedTypeNames?.Contains("JsonExceptionConverter") != true &&
            _document.Operations.Any(o => o.Operation.ActualResponses.Any(r => r.Value.Schema?.InheritsSchema(_exceptionSchema) == true));
    }
}