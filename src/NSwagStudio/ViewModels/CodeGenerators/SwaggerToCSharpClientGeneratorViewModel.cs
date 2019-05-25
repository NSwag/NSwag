//-----------------------------------------------------------------------
// <copyright file="MainWindowModel.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using NJsonSchema.CodeGeneration.CSharp;
using NSwag.CodeGeneration.CSharp;
using NSwag.Commands.CodeGeneration;

namespace NSwagStudio.ViewModels.CodeGenerators
{
    public class SwaggerToCSharpClientGeneratorViewModel : ViewModelBase
    {
        private string _clientCode;
        private SwaggerToCSharpClientCommand _command = new SwaggerToCSharpClientCommand();

        /// <summary>Gets the settings.</summary>
        public SwaggerToCSharpClientCommand Command
        {
            get { return _command; }
            set
            {
                if (Set(ref _command, value))
                    RaiseAllPropertiesChanged();
            }
        }

        /// <summary>Gets the list of operation modes. </summary>
        public OperationGenerationMode[] OperationGenerationModes { get; } = Enum.GetNames(typeof(OperationGenerationMode))
            .Select(t => (OperationGenerationMode)Enum.Parse(typeof(OperationGenerationMode), t))
            .ToArray();

        /// <summary>Gets the list of class styles. </summary>
        public CSharpClassStyle[] ClassStyles { get; } = Enum.GetNames(typeof(CSharpClassStyle))
            .Select(t => (CSharpClassStyle)Enum.Parse(typeof(CSharpClassStyle), t))
            .ToArray();

        /// <summary>Gets the list of cancellation token options. </summary>
        public CancellationTokenParameterOption[] CancellationTokenParameterOptions { get; } = Enum.GetNames(typeof(CancellationTokenParameterOption))
            .Select(t => (CancellationTokenParameterOption)Enum.Parse(typeof(CancellationTokenParameterOption), t))
            .ToArray();

        /// <summary>Gets or sets the client code. </summary>
        public string ClientCode
        {
            get { return _clientCode; }
            set { Set(ref _clientCode, value); }
        }
    }
}
