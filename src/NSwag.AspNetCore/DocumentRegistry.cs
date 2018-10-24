﻿//-----------------------------------------------------------------------
// <copyright file="DocumentRegistry.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NSwag.AspNetCore
{
    internal class DocumentRegistry
    {
        private readonly Dictionary<string, OpenApiDocumentSettings> _documentSettings;

        public DocumentRegistry(IEnumerable<OpenApiDocumentSettings> documentSettings)
        {
            _documentSettings = new Dictionary<string, OpenApiDocumentSettings>(StringComparer.Ordinal);
            foreach (var settings in documentSettings)
            {
                if (_documentSettings.ContainsKey(settings.DocumentName))
                {
                    throw new InvalidOperationException($"Repeated document name '{settings.DocumentName}' in " +
                        "registry. Open API document names must be unique.");
                }

                _documentSettings[settings.DocumentName] = settings;
            }

            DocumentNames = new ReadOnlyCollection<string>(_documentSettings.Keys.ToList());
        }

        public IReadOnlyCollection<string> DocumentNames { get; }

        public OpenApiDocumentSettings this[string documentName]
        {
            get
            {
                if (documentName == null)
                {
                    throw new ArgumentNullException(nameof(documentName));
                }

                _documentSettings.TryGetValue(documentName, out var document);
                return document;
            }
        }
    }
}
