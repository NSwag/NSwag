namespace NSwag.AspNet.Versioned.Owin.Middlewares
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AspNet.Owin;
    using Microsoft.Owin;
    using Microsoft.Web.Http.Description;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using NSwag;
    using SwaggerGeneration;
    using SwaggerGeneration.WebApi.Versioned;

    public class VersionedSwaggerDocumentMiddleware : OwinMiddleware
    {
        private readonly OwinMiddleware _next;
        private readonly SwaggerSettings<VersionedWebApiToSwaggerGeneratorSettings> _settings;
        private readonly SwaggerJsonSchemaGenerator _schemaGenerator;

        private Exception _schemaException;
        private DateTimeOffset _schemaTimestamp;
        private readonly VersionedApiExplorer _explorer;
        private string _schemaJson;

        /// <summary>Initializes a new instance of the <see cref="VersionedSwaggerDocumentMiddleware"/> class.</summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="explorer">The VersionedApiExplorer from which to generate the Swagger.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="schemaGenerator">The schema generator.</param>
        public VersionedSwaggerDocumentMiddleware(OwinMiddleware next, VersionedApiExplorer explorer, SwaggerSettings<VersionedWebApiToSwaggerGeneratorSettings> settings, SwaggerJsonSchemaGenerator schemaGenerator) : base(next)
        {
            _explorer = explorer;
            _next = next;
            _settings = settings;
            _schemaGenerator = schemaGenerator;
        }

        /// <summary>Generates the Swagger specification.</summary>
        /// <param name="context">The context.</param>
        /// <returns>The Swagger specification.</returns>
        protected virtual async Task<string> GenerateSwaggerAsync(IOwinContext context)
        {
            if (_schemaException != null && _schemaTimestamp + _settings.ExceptionCacheTime > DateTimeOffset.UtcNow)
                throw _schemaException;

            if (_schemaJson != null)
                return _schemaJson;

            string schemaJson;
            try {
                var generator = new VersionedWebApiToSwaggerGenerator(_explorer, _settings.GeneratorSettings, _schemaGenerator);
                var document = await generator.GenerateAsync();

                document.Host = context.Request.Host.Value ?? "";
                document.Schemes.Add(context.Request.Scheme == "http" ? SwaggerSchema.Http : SwaggerSchema.Https);

                _settings.PostProcess?.Invoke(document);
                schemaJson = document.ToJson();
                _schemaException = null;
                _schemaTimestamp = DateTimeOffset.UtcNow;
            }
             // If an exception is thrown cache the exception and try later.
            catch (Exception exception)
            {
                _schemaException = exception;
                _schemaTimestamp = DateTimeOffset.UtcNow;
                throw _schemaException;
            }

            _schemaJson = schemaJson;
            return schemaJson;
        }

        public override async Task Invoke(IOwinContext context)
        {
            if(context.Request.Path.HasValue){
                var requestPath = context.Request.Path.Value;
                if (requestPath.EndsWith(_settings.DocumentPath))
                {
                    var sSchemaJson = await GenerateSwaggerAsync(context);
                    context.Response.StatusCode = 200;
                    context.Response.Headers["Content-Type"] = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(sSchemaJson);
                    return;
                }
            }
            await _next.Invoke(context);
        }

    }
}