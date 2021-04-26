//-----------------------------------------------------------------------
// <copyright file="SwaggerSettings.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using Newtonsoft.Json;
using NSwag.Generation;

#if AspNetOwin
using NSwag.Generation.WebApi;
using Microsoft.Owin;

namespace NSwag.AspNet.Owin
#else
namespace NSwag.AspNetCore
#endif
{
    // TODO: Remove this class in v13, only used for legacy Web API middlewares

    /// <summary>The settings for UseSwagger.</summary>
#if AspNetOwin
    public class SwaggerSettings<T>
        where T : OpenApiDocumentGeneratorSettings, new()
#else
    public class SwaggerSettings
#endif
    {
        /// <summary>Initializes a new instance of the class.</summary>
        public SwaggerSettings()
        {
#if AspNetOwin
            GeneratorSettings = new T();
#endif
        }

#if AspNetOwin
        /// <summary>Gets the generator settings.</summary>
        public T GeneratorSettings { get; }
#endif

        /// <summary>Gets or sets the OWIN base path (when mapped via app.MapOwinPath()) (must start with '/').</summary>
#if !AspNetOwin
        [Obsolete("This property is ignored when using AspNetCoreToSwaggerGenerator and will be removed eventually.")]
#endif
        public string MiddlewareBasePath { get; set; }

#if !AspNetOwin
        /// <summary>Gets or sets the Swagger document route (must start with '/', default: '/swagger/{documentName}/swagger.json').</summary>
        /// <remarks>May contain '{documentName}' placeholder to register multiple routes.</remarks>
        public string DocumentPath { get; set; } = "/swagger/{documentName}/swagger.json";
#else
        /// <summary>Gets or sets the Swagger document route (must start with '/', default: '/swagger/v1/swagger.json').</summary>
        public string DocumentPath { get; set; } = "/swagger/v1/swagger.json";
#endif

#if AspNetOwin
        /// <summary>Gets or sets the Swagger post process action.</summary>
        public Action<OpenApiDocument> PostProcess { get; set; }

        /// <summary>Gets or sets for how long a <see cref="Exception"/> caught during schema generation is cached.</summary>
        public TimeSpan ExceptionCacheTime { get; set; } = TimeSpan.FromSeconds(10);
#endif

#pragma warning disable 618
        internal virtual string ActualSwaggerDocumentPath => DocumentPath.Substring(MiddlewareBasePath?.Length ?? 0);
#pragma warning restore 618

#if AspNetOwin
        internal T CreateGeneratorSettings(JsonSerializerSettings serializerSettings, object mvcOptions)
        {
            GeneratorSettings.ApplySettings(serializerSettings, mvcOptions);
            return GeneratorSettings;
        }
#endif
    }
}
