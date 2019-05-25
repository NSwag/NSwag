//-----------------------------------------------------------------------
// <copyright file="SwaggerToCSharpClientGeneratorSettings.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;

namespace NSwag.CodeGeneration.CSharp
{
    /// <summary>Settings for the <see cref="SwaggerToCSharpClientGenerator"/>.</summary>
    public class SwaggerToCSharpClientGeneratorSettings : SwaggerToCSharpGeneratorSettings
    {
        /// <summary>Initializes a new instance of the <see cref="SwaggerToCSharpClientGeneratorSettings"/> class.</summary>
        public SwaggerToCSharpClientGeneratorSettings()
        {
            ClassName = "{controller}Client";

            GenerateExceptionClasses = true;
            ExceptionClass = "SwaggerException";
            ClientClassAccessModifier = "public";
            UseBaseUrl = true;
            HttpClientType = "System.Net.Http.HttpClient";
            WrapDtoExceptions = true;
            DisposeHttpClient = true;
            ParameterDateTimeFormat = "s";
            GenerateUpdateJsonSerializerSettingsMethod = true;
            QueryNullValue = "";
            GenerateBaseUrlProperty = true;
            ExposeJsonSerializerSettings = false;
            InjectHttpClient = true;
            ProtectedMethods = new string[0];
            InterfaceCancellationTokenParameter = CancellationTokenParameterOption.Both;
        }

        /// <summary>Gets or sets the full name of the base class.</summary>
        public string ClientBaseClass { get; set; }

        /// <summary>Gets or sets the full name of the configuration class (<see cref="ClientBaseClass"/> must be set).</summary>
        public string ConfigurationClass { get; set; }

        /// <summary>Gets or sets a value indicating whether to generate exception classes (default: true).</summary>
        public bool GenerateExceptionClasses { get; set; }

        /// <summary>Gets or sets the name of the exception class (supports the '{controller}' placeholder, default 'SwaggerException').</summary>
        public string ExceptionClass { get; set; }

        /// <summary>Gets or sets a value indicating whether an HttpClient instance is injected into the client (default: true).</summary>
        public bool InjectHttpClient { get; set; }

        /// <summary>Gets or sets a value indicating whether to dispose the HttpClient (injected HttpClient is never disposed, default: true).</summary>
        public bool DisposeHttpClient { get; set; }

        /// <summary>Gets or sets the list of methods with a protected access modifier ("classname.methodname").</summary>
        public string[] ProtectedMethods { get; set; }

        /// <summary>Gets or sets a value indicating whether to call CreateHttpClientAsync on the base class to create a new HttpClient instance (cannot be used when the HttpClient is injected).</summary>
        public bool UseHttpClientCreationMethod { get; set; }

        /// <summary>Gets or sets a value indicating whether to call CreateHttpRequestMessageAsync on the base class to create a new HttpRequestMethod.</summary>
        public bool UseHttpRequestMessageCreationMethod { get; set; }

        /// <summary>Gets or sets a value indicating whether DTO exceptions are wrapped in a SwaggerException instance (default: true).</summary>
        public bool WrapDtoExceptions { get; set; }

        /// <summary>Gets or sets the client class access modifier (default: public).</summary>
        public string ClientClassAccessModifier { get; set; }

        /// <summary>Gets or sets a value indicating whether to use and expose the base URL (default: true).</summary>
        public bool UseBaseUrl { get; set; }

        /// <summary>Gets or sets a value indicating whether to generate the BaseUrl property, must be defined on the base class otherwise (default: true).</summary>
        public bool GenerateBaseUrlProperty { get; set; }

        /// <summary>Gets or sets a value indicating whether to generate synchronous methods (not recommended, default: false).</summary>
        public bool GenerateSyncMethods { get; set; }

        /// <summary>
        /// Gets or sets the HttpClient type which will be used in the generation of the client code. By default the System.Net.Http.HttpClient
        /// will be used, but this can be overridden. Just keep in mind that the type you specify has the same default HttpClient method signatures.
        /// </summary>
        public string HttpClientType { get; set; }

        /// <summary>Gets or sets the format for DateTime type method parameters (default: "s").</summary>
        public string ParameterDateTimeFormat { get; set; }

        /// <summary>Gets or sets a value indicating whether to generate the UpdateJsonSerializerSettings method (must be implemented in the base class otherwise, default: true).</summary>
        public bool GenerateUpdateJsonSerializerSettingsMethod { get; set; }

        /// <summary>Gets or sets a value indicating whether to serialize the type information in a $type property (not recommended, also sets TypeNameHandling = Auto).</summary>
        public bool SerializeTypeInformation { get; set; }

        /// <summary>Gets or sets the null value used for query parameters which are null (default: '').</summary>
        public string QueryNullValue { get; set; }

        /// <summary>Gets or sets a value indicating whether to expose the JsonSerializerSettings property (default: false).</summary>
        public bool ExposeJsonSerializerSettings { get; set; }

        /// <summary>Gets or sets a value indicating whether to generate async method with CancellationToken as last parameter in interface. (default: both)</summary>
        public CancellationTokenParameterOption InterfaceCancellationTokenParameter { get; set; }
    }
    
    /// <summary>
    /// Whether to generate async method with CancellationToken as last parameter in interface.
    /// </summary>
    [Flags]
    public enum CancellationTokenParameterOption
    {
        /// <summary>
        /// Generate async method with CancellationToken in interface
        /// </summary>
        Yes = 1,
        /// <summary>
        /// Generate async method without CancellationToken in interface
        /// </summary>
        No = 2,
        /// <summary>
        /// Generate both async methods
        /// </summary>
        Both = Yes | No
    }
}
