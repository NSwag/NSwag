//-----------------------------------------------------------------------
// <copyright file="CSharpControllerTemplateModel.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace NSwag.CodeGeneration.CSharp.Models
{
    /// <summary>The CSharp controller template model.</summary>
    public class CSharpControllerTemplateModel : CSharpTemplateModelBase
    {
        private readonly SwaggerToCSharpControllerGeneratorSettings _settings;
        private readonly SwaggerDocument _document;

        /// <summary>Initializes a new instance of the <see cref="CSharpControllerTemplateModel" /> class.</summary>
        /// <param name="controllerName">Name of the controller.</param>
        /// <param name="operations">The operations.</param>
        /// <param name="document">The document.</param>
        /// <param name="settings">The settings.</param>
        public CSharpControllerTemplateModel(
            string controllerName,
            IEnumerable<CSharpOperationModel> operations,
            SwaggerDocument document,
            SwaggerToCSharpControllerGeneratorSettings settings)
            : base(controllerName, settings)
        {
            _document = document;
            _settings = settings;

            Class = controllerName;
            Operations = operations;

            BaseClass = _settings.ControllerBaseClass?.Replace("{controller}", controllerName);
        }

        /// <summary>Gets or sets the class name.</summary>
        public string Class { get; }

        /// <summary>Gets a value indicating whether the controller has a base class.</summary>
        public bool HasBaseClass => !string.IsNullOrEmpty(BaseClass);

        /// <summary>Gets the ASP.NET framework namespace.</summary>
        public string AspNetNamespace => IsAspNetCore ? "Microsoft.AspNetCore.Mvc" : "System.Web.Http";

        /// <summary>Gets or sets a value indicating whether the output should target ASP.NET Core.</summary>
        public bool IsAspNetCore => _settings.ControllerTarget == CSharpControllerTarget.AspNetCore;

        /// <summary>Gets or sets a value indicating whether the output should target ASP.NET MVC.</summary>
        public bool IsAspNet => _settings.ControllerTarget == CSharpControllerTarget.AspNet;

        /// <summary>Gets the base class.</summary>
        public string BaseClass { get; }

        /// <summary>Gets or sets the service base URL.</summary>
        public string BaseUrl => _document.BaseUrl;

        /// <summary>Gets or sets a value indicating whether the controller has operations.</summary>
        public bool HasOperations => Operations.Any();

        /// <summary>Gets or sets the operations.</summary>
        public IEnumerable<CSharpOperationModel> Operations { get; set; }

        /// <summary>Gets or sets a value indicating whether the controller has a base path.</summary>
        public bool HasBasePath => !string.IsNullOrEmpty(BasePath);

        /// <summary>Gets or sets the base path.</summary>
        public string BasePath => _document.BasePath?.TrimStart('/');

        /// <summary>Gets a value indicating whether to generate optional parameters.</summary>
        public bool GenerateOptionalParameters => _settings.GenerateOptionalParameters;

        /// <summary>Gets a value indicating whether to generate partial controllers.</summary>
        public bool GeneratePartialControllers => _settings.ControllerStyle == CSharpControllerStyle.Partial;

        /// <summary>Gets a value indicating whether to generate abstract controllers.</summary>
        public bool GenerateAbstractControllers => _settings.ControllerStyle == CSharpControllerStyle.Abstract;

        /// <summary>Gets a value indicating whether to allow adding cancellation token.</summary>
        public bool UseCancellationToken => _settings.UseCancellationToken;

        /// <summary>Gets the Title.</summary>
        public string Title => _document.Info.Title;

        /// <summary>Gets the Description.</summary>
        public string Description => _document.Info.Description;

        /// <summary>Gets the API version.</summary>
        public string Version => _document.Info.Version;

		/// <summary>Gets the setting which indicates if authorization attributes should be generated.</summary>
		public bool GenerateAuthorizationAttributes => _settings.GenerateAuthorizationAttributes;

		/// <summary>Checks if any authentication scheme are mapped to this controller.</summary>
		public bool HasSecurity { get { return GetHasSecurity(); } }

		/// <summary>Gets a comma delimited list of authentication scheme names mapped to this controller.</summary>
		public string AuthenticationSchemes { get { return GetAuthenticationSchemes(); } }

		private bool GetHasSecurity()
		{
			return (_document.Security.Count > 0);
		}

		private string GetAuthenticationSchemes()
		{
			string authenticationSchemes = "";

			foreach (Dictionary<string, IEnumerable<string>> authenticationSchemeDictionary in _document.Security)
			{
				foreach (KeyValuePair<string, IEnumerable<string>> authenticationScheme in authenticationSchemeDictionary)
				{
					authenticationSchemes += (string.IsNullOrWhiteSpace(authenticationSchemes)) ? "" : ",";
					authenticationSchemes += authenticationScheme.Key;
				}
			}

			return authenticationSchemes;
		}
	}
}
