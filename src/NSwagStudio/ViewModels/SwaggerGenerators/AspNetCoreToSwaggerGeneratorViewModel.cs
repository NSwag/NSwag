//-----------------------------------------------------------------------
// <copyright file="WebApiToSwaggerGeneratorViewModel.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using MyToolkit.Command;
using NJsonSchema;
using NJsonSchema.Generation;
using NSwag;
using NSwag.Commands;
using NSwag.Commands.Generation.AspNetCore;

namespace NSwagStudio.ViewModels.SwaggerGenerators
{
    public class AspNetCoreToSwaggerGeneratorViewModel : ViewModelBase
    {
        private string[] _allControllerNames = { };
        private AspNetCoreToSwaggerCommand _command = new AspNetCoreToSwaggerCommand();
        private NSwagDocument _document;

        /// <summary>Initializes a new instance of the <see cref="WebApiToSwaggerGeneratorViewModel"/> class.</summary>
        public AspNetCoreToSwaggerGeneratorViewModel()
        {
            BrowseAssemblyCommand = new AsyncRelayCommand(BrowseAssembly);
        }

        /// <summary>Gets the default enum handlings. </summary>
        public EnumHandling[] EnumHandlings { get; } = Enum.GetNames(typeof(EnumHandling))
            .Select(t => (EnumHandling)Enum.Parse(typeof(EnumHandling), t))
            .ToArray();

        /// <summary>Gets the default property name handlings. </summary>
        public PropertyNameHandling[] PropertyNameHandlings { get; } = Enum.GetNames(typeof(PropertyNameHandling))
            .Select(t => (PropertyNameHandling)Enum.Parse(typeof(PropertyNameHandling), t))
            .ToArray();

        /// <summary>Gets the reference type null handlings. </summary>
        public ReferenceTypeNullHandling[] ReferenceTypeNullHandlings { get; } = Enum.GetNames(typeof(ReferenceTypeNullHandling))
            .Select(t => (ReferenceTypeNullHandling)Enum.Parse(typeof(ReferenceTypeNullHandling), t))
            .ToArray();

        /// <summary>Gets new line behaviors. </summary>
        public NewLineBehavior[] NewLineBehaviors { get; } = Enum.GetNames(typeof(NewLineBehavior))
            .Select(t => (NewLineBehavior)Enum.Parse(typeof(NewLineBehavior), t))
            .ToArray();

        /// <summary>Gets the output types. </summary>
        public SchemaType[] OutputTypes { get; } = { SchemaType.Swagger2, SchemaType.OpenApi3 };

        /// <summary>Gets or sets the command to browse for an assembly.</summary>
        public AsyncRelayCommand BrowseAssemblyCommand { get; set; }

        /// <summary>Gets or sets the generator settings. </summary>
        public AspNetCoreToSwaggerCommand Command
        {
            get { return _command; }
            set
            {
                if (Set(ref _command, value))
                    RaiseAllPropertiesChanged();
            }
        }

        /// <summary>Gets or sets the document.</summary>
        public NSwagDocument Document
        {
            get { return _document; }
            set { Set(ref _document, value); }
        }

        /// <summary>Gets or sets the assembly path. </summary>
        public string[] AssemblyPaths
        {
            get { return Command.AssemblyPaths; }
            set
            {
                Command.AssemblyPaths = value;
                RaisePropertyChanged(() => AssemblyPaths);
            }
        }

        public async Task<string> GenerateSwaggerAsync()
        {
            return await RunTaskAsync(async () =>
            {
                return await Task.Run(async () =>
                {
                    var document = (OpenApiDocument)await Command.RunAsync(null, null).ConfigureAwait(false);
                    return document?.ToJson();
                });
            });
        }

        private Task BrowseAssembly()
        {
            var dlg = new OpenFileDialog();
            dlg.DefaultExt = ".dll"; //
            dlg.Filter = ".NET Assemblies (*.dll;*.exe)|*.dll;*.exe";
            if (dlg.ShowDialog() == true)
            {
                AssemblyPaths = new[] { dlg.FileName };
            }

            return Task.CompletedTask;
        }
    }
}
