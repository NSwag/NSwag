//-----------------------------------------------------------------------
// <copyright file="TypeScriptClientTemplateModel.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using NJsonSchema.CodeGeneration.TypeScript;

namespace NSwag.CodeGeneration.TypeScript.Models
{
    /// <summary>The TypeScript client template model.</summary>
    public class TypeScriptClientTemplateModel
    {
        private readonly TypeScriptExtensionCode _extensionCode;
        private readonly TypeScriptClientGeneratorSettings _settings;
        private readonly OpenApiDocument _document;

        /// <summary>Initializes a new instance of the <see cref="TypeScriptClientTemplateModel" /> class.</summary>
        /// <param name="controllerName">Name of the controller.</param>
        /// <param name="controllerClassName">Name of the controller.</param>
        /// <param name="operations">The operations.</param>
        /// <param name="extensionCode">The extension code.</param>
        /// <param name="document">The Swagger document.</param>
        /// <param name="settings">The settings.</param>
        public TypeScriptClientTemplateModel(
            string controllerName,
            string controllerClassName,
            IEnumerable<TypeScriptOperationModel> operations,
            TypeScriptExtensionCode extensionCode,
            OpenApiDocument document,
            TypeScriptClientGeneratorSettings settings)
        {
            _extensionCode = extensionCode;
            _settings = settings;
            _document = document;

            Class = controllerClassName;
            Operations = operations;

            BaseClass = _settings.ClientBaseClass?.Replace("{controller}", controllerName);
            Framework = new TypeScriptFrameworkModel(settings);
        }

        /// <summary>Gets framework specific information.</summary>
        public TypeScriptFrameworkModel Framework { get; set; }

        /// <summary>Gets the class name.</summary>
        public string Class { get; }

        /// <summary>Gets a value indicating whether the client class has a base class.</summary>
        public bool HasBaseClass => !string.IsNullOrEmpty(BaseClass);

        /// <summary>Gets the client base class.</summary>
        public string BaseClass { get; }

        /// <summary>Gets or sets a value indicating whether to use the getBaseUrl(defaultUrl: string) from the base class.</summary>
        public bool UseGetBaseUrlMethod => _settings.UseGetBaseUrlMethod;

        /// <summary>Gets the configuration class name.</summary>
        public string ConfigurationClass => _settings.ConfigurationClass;

        /// <summary>Gets a value indicating whether the client class has a base class.</summary>
        public bool HasConfigurationClass => HasBaseClass && !string.IsNullOrEmpty(ConfigurationClass);

        /// <summary>Gets or sets a value indicating whether to call 'transformOptions' on the base class or extension class.</summary>
        public bool UseTransformOptionsMethod => _settings.UseTransformOptionsMethod;

        /// <summary>Gets or sets a value indicating whether to call 'transformResult' on the base class or extension class.</summary>
        public bool UseTransformResultMethod => _settings.UseTransformResultMethod;

        /// <summary>Gets a value indicating whether to generate optional parameters.</summary>
        public bool GenerateOptionalParameters => _settings.GenerateOptionalParameters;

        /// <summary>Gets a value indicating whether the client is extended with an extension class.</summary>
        public bool HasExtensionCode => _settings.TypeScriptGeneratorSettings.ExtendedClasses?.Any(c => c == Class) == true;

        /// <summary>Gets the extension body code.</summary>
        public string ExtensionCode => _extensionCode.GetExtensionClassBody(Class);

        /// <summary>Gets or sets a value indicating whether the extension code has a constructor and no constructor has to be generated.</summary>
        public bool HasExtendedConstructor => HasExtensionCode && ExtensionCode.Contains("constructor(");

        /// <summary>Gets a value indicating whether the client has operations.</summary>
        public bool HasOperations => Operations.Any();

        /// <summary>Gets the operations.</summary>
        public IEnumerable<TypeScriptOperationModel> Operations { get; }

        /// <summary>Gets the service base URL.</summary>
        public string BaseUrl => _document.BaseUrl;

        /// <summary>Gets a value indicating whether to generate client interfaces.</summary>
        public bool GenerateClientInterfaces => _settings.GenerateClientInterfaces;

        /// <summary>Gets the promise type.</summary>
        public string PromiseType => _settings.PromiseType == TypeScript.PromiseType.Promise ? "Promise" : "Q.Promise";

        /// <summary>Gets the promise constructor code.</summary>
        public string PromiseConstructor => _settings.PromiseType == TypeScript.PromiseType.Promise ? "new Promise" : "Q.Promise";

        /// <summary>Gets or sets a value indicating whether to use Aurelia HTTP injection.</summary>
        public bool UseAureliaHttpInjection => _settings.Template == TypeScriptTemplate.Aurelia;

        /// <summary>Gets a value indicating whether the target TypeScript version supports strict null checks.</summary>
        public bool SupportsStrictNullChecks => _settings.TypeScriptGeneratorSettings.TypeScriptVersion >= 2.0m;

        /// <summary>Gets or sets a value indicating whether DTO exceptions are wrapped in a SwaggerException instance.</summary>
        public bool WrapDtoExceptions => _settings.WrapDtoExceptions;

        /// <summary>Gets or sets the null value used for query parameters which are null.</summary>
        public string QueryNullValue => _settings.QueryNullValue;

        /// <summary>Gets whether the export keyword should be added to all classes and enums.</summary>
        public bool ExportTypes => _settings.TypeScriptGeneratorSettings.ExportTypes;

        /// <summary>Gets a value indicating whether to use the AbortSignal (Fetch/Aurelia template only, default: false).</summary>
        public bool UseAbortSignal => _settings.UseAbortSignal;

        /// <summary>Gets or sets a value indicating whether DTO exceptions are wrapped in a SwaggerException instance.</summary>
        public bool NamedParameters => _settings.NamedParameters;
    }
}