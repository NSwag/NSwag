//-----------------------------------------------------------------------
// <copyright file="SwaggerToCSharpClientCommand.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NConsole;
using NSwag.CodeGeneration;
using NSwag.CodeGeneration.CSharp;

#pragma warning disable 1591

namespace NSwag.Commands.CodeGeneration
{
    [Command(Name = "swagger2csclient", Description = "Generates CSharp client code from a Swagger specification.")]
    public class SwaggerToCSharpClientCommand : SwaggerToCSharpCommandBase<SwaggerToCSharpClientGeneratorSettings>
    {
        public SwaggerToCSharpClientCommand() : base(new SwaggerToCSharpClientGeneratorSettings())
        {
        }

        [Argument(Name = "ClientBaseClass", IsRequired = false, Description = "The client base class (empty for no base class).")]
        public string ClientBaseClass
        {
            get { return Settings.ClientBaseClass; }
            set { Settings.ClientBaseClass = value; }
        }

        [Argument(Name = "ConfigurationClass", IsRequired = false, Description = "The configuration class. The setting ClientBaseClass must be set. (empty for no configuration class).")]
        public string ConfigurationClass
        {
            get { return Settings.ConfigurationClass; }
            set { Settings.ConfigurationClass = value; }
        }

        [Argument(Name = "GenerateClientClasses", IsRequired = false, Description = "Specifies whether generate client classes.")]
        public bool GenerateClientClasses
        {
            get { return Settings.GenerateClientClasses; }
            set { Settings.GenerateClientClasses = value; }
        }

        [Argument(Name = "GenerateClientInterfaces", IsRequired = false, Description = "Specifies whether generate interfaces for the client classes.")]
        public bool GenerateClientInterfaces
        {
            get { return Settings.GenerateClientInterfaces; }
            set { Settings.GenerateClientInterfaces = value; }
        }

        [Argument(Name = "InjectHttpClient", IsRequired = false, Description = "Specifies whether an HttpClient instance is injected (default: true).")]
        public bool InjectHttpClient
        {
            get { return Settings.InjectHttpClient; }
            set { Settings.InjectHttpClient = value; }
        }

        [Argument(Name = "DisposeHttpClient", IsRequired = false, Description = "Specifies whether to dispose the HttpClient (injected HttpClient is never disposed).")]
        public bool DisposeHttpClient
        {
            get { return Settings.DisposeHttpClient; }
            set { Settings.DisposeHttpClient = value; }
        }

        [Argument(Name = "ProtectedMethods", IsRequired = false, Description = "List of methods with a protected access modifier ('classname.methodname').")]
        public string[] ProtectedMethods
        {
            get { return Settings.ProtectedMethods; }
            set { Settings.ProtectedMethods = value; }
        }

        [Argument(Name = "GenerateExceptionClasses", IsRequired = false, Description = "Specifies whether to generate exception classes (default: true).")]
        public bool GenerateExceptionClasses
        {
            get { return Settings.GenerateExceptionClasses; }
            set { Settings.GenerateExceptionClasses = value; }
        }

        [Argument(Name = "ExceptionClass", IsRequired = false, Description = "The exception class (default 'SwaggerException', may use '{controller}' placeholder).")]
        public string ExceptionClass
        {
            get { return Settings.ExceptionClass; }
            set { Settings.ExceptionClass = value; }
        }

        [Argument(Name = "WrapDtoExceptions", IsRequired = false, Description = "Specifies whether DTO exceptions are wrapped in a SwaggerException instance (default: true).")]
        public bool WrapDtoExceptions
        {
            get { return Settings.WrapDtoExceptions; }
            set { Settings.WrapDtoExceptions = value; }
        }

        [Argument(Name = "UseHttpClientCreationMethod", IsRequired = false, Description = "Specifies whether to call CreateHttpClientAsync on the base class to create a new HttpClient.")]
        public bool UseHttpClientCreationMethod
        {
            get { return Settings.UseHttpClientCreationMethod; }
            set { Settings.UseHttpClientCreationMethod = value; }
        }

        [Argument(Name = "HttpClientType", IsRequired = false, Description = "Specifies the HttpClient type. By default the 'System.Net.Http.HttpClient' is used.")]
        public string HttpClientType
        {
            get { return Settings.HttpClientType; }
            set { Settings.HttpClientType = value; }
        }

        [Argument(Name = "UseHttpRequestMessageCreationMethod", IsRequired = false,
                  Description = "Specifies whether to call CreateHttpRequestMessageAsync on the base class to create a new HttpRequestMethod.")]
        public bool UseHttpRequestMessageCreationMethod
        {
            get { return Settings.UseHttpRequestMessageCreationMethod; }
            set { Settings.UseHttpRequestMessageCreationMethod = value; }
        }

        [Argument(Name = "UseBaseUrl", IsRequired = false,
                  Description = "Specifies whether to use and expose the base URL (default: true).")]
        public bool UseBaseUrl
        {
            get { return Settings.UseBaseUrl; }
            set { Settings.UseBaseUrl = value; }
        }

        [Argument(Name = nameof(GenerateBaseUrlProperty), IsRequired = false,
                  Description = "Specifies whether to generate the BaseUrl property, must be defined on the base class otherwise (default: true).")]
        public bool GenerateBaseUrlProperty
        {
            get { return Settings.GenerateBaseUrlProperty; }
            set { Settings.GenerateBaseUrlProperty = value; }
        }

        [Argument(Name = "GenerateSyncMethods", IsRequired = false,
                  Description = "Specifies whether to generate synchronous methods (not recommended, default: false).")]
        public bool GenerateSyncMethods
        {
            get { return Settings.GenerateSyncMethods; }
            set { Settings.GenerateSyncMethods = value; }
        }

        [Argument(Name = nameof(ExposeJsonSerializerSettings), IsRequired = false,
            Description = "Specifies whether to expose the JsonSerializerSettings property (default: false).")]
        public bool ExposeJsonSerializerSettings
        {
            get { return Settings.ExposeJsonSerializerSettings; }
            set { Settings.ExposeJsonSerializerSettings = value; }
        }

        [Argument(Name = "ClientClassAccessModifier", IsRequired = false, Description = "The client class access modifier (default: public).")]
        public string ClientClassAccessModifier
        {
            get { return Settings.ClientClassAccessModifier; }
            set { Settings.ClientClassAccessModifier = value; }
        }

        [Argument(Name = "TypeAccessModifier", IsRequired = false, Description = "The DTO class/enum access modifier (default: public).")]
        public string TypeAccessModifier
        {
            get { return Settings.CSharpGeneratorSettings.TypeAccessModifier; }
            set { Settings.CSharpGeneratorSettings.TypeAccessModifier = value; }
        }

        [Argument(Name = "GenerateContractsOutput", IsRequired = false,
                  Description = "Specifies whether to generate contracts output (interface and models in a separate file set with the ContractsOutput parameter).")]
        public bool GenerateContractsOutput { get; set; }

        [Argument(Name = "ContractsNamespace", IsRequired = false, Description = "The contracts .NET namespace.")]
        public string ContractsNamespace { get; set; }

        [Argument(Name = "ContractsOutput", IsRequired = false, 
                  Description = "The contracts output file path (optional, if no path is set then a single file with the implementation and contracts is generated).")]
        public string ContractsOutputFilePath { get; set; }

        [Argument(Name = "ParameterDateTimeFormat", IsRequired = false,
                  Description = "Specifies the format for DateTime type method parameters (default: s).")]
        public string ParameterDateTimeFormat
        {
            get { return Settings.ParameterDateTimeFormat; }
            set { Settings.ParameterDateTimeFormat = value; }
        }

        [Argument(Name = "GenerateUpdateJsonSerializerSettingsMethod", IsRequired = false,
            Description = "Generate the UpdateJsonSerializerSettings method (must be implemented in the base class otherwise, default: true).")]
        public bool GenerateUpdateJsonSerializerSettingsMethod
        {
            get { return Settings.GenerateUpdateJsonSerializerSettingsMethod; }
            set { Settings.GenerateUpdateJsonSerializerSettingsMethod = value; }
        }

        [Argument(Name = "SerializeTypeInformation", IsRequired = false,
            Description = "Serialize the type information in a $type property (not recommended, also sets TypeNameHandling = Auto, default: true).")]
        public bool SerializeTypeInformation
        {
            get { return Settings.SerializeTypeInformation; }
            set { Settings.SerializeTypeInformation = value; }
        }

        [Argument(Name = nameof(QueryNullValue), IsRequired = false, 
            Description = "The null value used for query parameters which are null (default: '').")]
        public string QueryNullValue
        {
            get { return Settings.QueryNullValue; }
            set { Settings.QueryNullValue = value; }
        }

        [Argument(Name = nameof(InterfaceCancellationTokenParameter), IsRequired = false, 
            Description = "Specifies whether to generate async method with CancellationToken as last parameter in interface.")]
        public CancellationTokenParameterOption InterfaceCancellationTokenParameter
        {
            get { return Settings.InterfaceCancellationTokenParameter; }
            set { Settings.InterfaceCancellationTokenParameter = value; }
        }

        public override async Task<object> RunAsync(CommandLineProcessor processor, IConsoleHost host)
        {
            var result = await RunAsync();
            foreach (var pair in result)
            {
                await TryWriteFileOutputAsync(pair.Key, host, () => pair.Value).ConfigureAwait(false);
            }

            return result;
        }

        public async Task<Dictionary<string, string>> RunAsync()
        {
            return await Task.Run(async () =>
            {
                var document = await GetInputSwaggerDocument().ConfigureAwait(false);
                var clientGenerator = new SwaggerToCSharpClientGenerator(document, Settings);

                if (GenerateContractsOutput)
                {
                    var result = new Dictionary<string, string>();
                    GenerateContracts(result, clientGenerator);
                    GenerateImplementation(result, clientGenerator);
                    return result;
                }
                else
                {
                    return new Dictionary<string, string>
                    {
                        { OutputFilePath ?? "Full", clientGenerator.GenerateFile(ClientGeneratorOutputType.Full) }
                    };
                }
            });
        }

        private void GenerateImplementation(Dictionary<string, string> result, SwaggerToCSharpClientGenerator clientGenerator)
        {
            var savedAdditionalNamespaceUsages = Settings.AdditionalNamespaceUsages?.ToArray();
            Settings.AdditionalNamespaceUsages =
                Settings.AdditionalNamespaceUsages?.Concat(new[] { ContractsNamespace }).ToArray() ?? new[] { ContractsNamespace };
            result[OutputFilePath ?? "Implementation"] = clientGenerator.GenerateFile(ClientGeneratorOutputType.Implementation);
            Settings.AdditionalNamespaceUsages = savedAdditionalNamespaceUsages;
        }

        private void GenerateContracts(Dictionary<string, string> result, SwaggerToCSharpClientGenerator clientGenerator)
        {
            var savedNamespace = Settings.CSharpGeneratorSettings.Namespace;
            Settings.CSharpGeneratorSettings.Namespace = ContractsNamespace;
            result[ContractsOutputFilePath ?? "Contracts"] = clientGenerator.GenerateFile(ClientGeneratorOutputType.Contracts);
            Settings.CSharpGeneratorSettings.Namespace = savedNamespace;
        }
    }
}
