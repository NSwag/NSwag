//-----------------------------------------------------------------------
// <copyright file="SwaggerSchemaResolver.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using NJsonSchema;
using NJsonSchema.Generation;

namespace NSwag
{
    /// <summary>Appends a JSON Schema to the Definitions of a Swagger document.</summary>
    public class OpenApiSchemaResolver : JsonSchemaResolver
    {
        private readonly ITypeNameGenerator _typeNameGenerator;

        private OpenApiDocument Document => (OpenApiDocument)RootObject;

        /// <summary>Initializes a new instance of the <see cref="OpenApiSchemaResolver" /> class.</summary>
        /// <param name="document">The Swagger document.</param>
        /// <param name="settings">The settings.</param>
        /// <exception cref="ArgumentNullException"><paramref name="document" /> is <see langword="null" /></exception>
        public OpenApiSchemaResolver(OpenApiDocument document, JsonSchemaGeneratorSettings settings)
            : base(document, settings)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            _typeNameGenerator = settings.TypeNameGenerator;
        }

        /// <summary>Appends the schema to the root object.</summary>
        /// <param name="schema">The schema to append.</param>
        /// <param name="typeNameHint">The type name hint.</param>
        public override void AppendSchema(JsonSchema schema, string typeNameHint, Type type)
        {
            if (!Document.Definitions.Values.Contains(schema))
            {
                var typeName = _typeNameGenerator.Generate(schema, typeNameHint, Document.Definitions.Keys);
                var attemptedTypeName = _typeNameGenerator.Generate(schema, typeNameHint, new List<string>() { });
                Document.DefinitionsMap.Add(new TypeDefinitionTracker()
                {
                    GeneratedTypeName = typeName,
                    AttemptedTypeName = attemptedTypeName,
                    Type = type
                });
                Document.Definitions[typeName] = schema;
            }
        }
    }
}