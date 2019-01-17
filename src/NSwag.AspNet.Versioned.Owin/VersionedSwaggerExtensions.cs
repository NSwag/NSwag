//-----------------------------------------------------------------------
// <copyright file="SwaggerExtensions.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

namespace NSwag.AspNet.Versioned.Owin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AspNet.Owin;
    using AspNet.Owin.Middlewares;
    using global::Owin;
    using Microsoft.Owin;
    using Microsoft.Owin.Extensions;
    using Microsoft.Owin.FileSystems;
    using Microsoft.Owin.StaticFiles;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Description;
    using Middlewares;
    using SwaggerGeneration;
    using SwaggerGeneration.WebApi;
    using SwaggerGeneration.WebApi.Versioned;

    /// <summary>Provides OWIN extensions to enable Swagger UI.</summary>
    public static class VersionedSwaggerExtensions
    {

        public static IAppBuilder UseVersionedSwagger(
            this IAppBuilder app,
            VersionedApiExplorer explorer,
            Action<SwaggerSettings<VersionedWebApiToSwaggerGeneratorSettings>> configure = null,
            SwaggerJsonSchemaGenerator schemaGenerator = null)
        {
            var settings = new SwaggerSettings<VersionedWebApiToSwaggerGeneratorSettings>();
            configure?.Invoke(settings);

            return app.Use(typeof(VersionedSwaggerDocumentMiddleware), explorer, settings, schemaGenerator ?? new SwaggerJsonSchemaGenerator(settings.GeneratorSettings));
        }

        /// <summary>Adds the Swagger generator and Swagger UI to the OWIN pipeline.</summary>
        /// <param name="app">The app.</param>
        /// <param name="explorer">The versioned API explorer.</param>
        /// <param name="configure">Configure the Swagger settings.</param>
        /// <param name="schemaGenerator">The schema generator.</param>
        /// <returns>The app builder.</returns>
        public static IAppBuilder UseVersionedSwaggerUi3(
            this IAppBuilder app,
            VersionedApiExplorer explorer,
            Action<SwaggerUi3Settings<VersionedWebApiToSwaggerGeneratorSettings>> configure = null,
            SwaggerJsonSchemaGenerator schemaGenerator = null)
        {
            var settings = new SwaggerUi3Settings<VersionedWebApiToSwaggerGeneratorSettings>();
            configure?.Invoke(settings);

            app.Use<VersionedSwaggerDocumentMiddleware>(explorer, settings, schemaGenerator ?? new SwaggerJsonSchemaGenerator(settings.GeneratorSettings));

            app.UseSwaggerUi3(settings.ToUi3SettingsAction());

            app.UseStageMarker(PipelineStage.MapHandler);
            return app;
        }

        private static Action<SwaggerUi3Settings<WebApiToSwaggerGeneratorSettings>> ToUi3SettingsAction(
            this SwaggerUi3Settings<VersionedWebApiToSwaggerGeneratorSettings> settings )
        {
            return delegate (SwaggerUi3Settings<WebApiToSwaggerGeneratorSettings> uiSettings)
            {
                var copyToProperties =
                    uiSettings.GetType().GetProperties();
                var copyFromType = settings.GetType();

                foreach ( var property in copyToProperties )
                {
                    if(property.CanWrite)
                        property.SetValue( uiSettings, copyFromType.GetProperty( property.Name )?.GetValue( settings ) );
                }

                foreach ( var route in settings.SwaggerRoutes )
                {
                    uiSettings.SwaggerRoutes.Add(route);
                }
            };
        }
    }
}
