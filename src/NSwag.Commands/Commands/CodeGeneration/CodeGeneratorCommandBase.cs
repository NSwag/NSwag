//-----------------------------------------------------------------------
// <copyright file="CodeGeneratorCommandBase.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/RicoSuter/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using NConsole;
using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.CodeGeneration;
using NSwag.CodeGeneration;

namespace NSwag.Commands.CodeGeneration
{
    public abstract class CodeGeneratorCommandBase<TSettings> : InputOutputCommandBase
        where TSettings : ClientGeneratorBaseSettings
    {
        protected CodeGeneratorCommandBase(TSettings settings)
        {
            Settings = settings;
        }

        [JsonIgnore]
        public TSettings Settings { get; }

        [Argument(Name = "TemplateDirectory", IsRequired = false, Description = "The Liquid template directory (experimental).")]
        public string TemplateDirectory
        {
            get { return Settings.CodeGeneratorSettings.TemplateDirectory; }
            set { Settings.CodeGeneratorSettings.TemplateDirectory = value; }
        }

        [Argument(Name = "TypeNameGenerator", IsRequired = false, Description = "The custom ITypeNameGenerator implementation type in the form 'assemblyName:fullTypeName' or 'fullTypeName').")]
        public string TypeNameGeneratorType { get; set; }

        [Argument(Name = "PropertyNameGeneratorType", IsRequired = false, Description = "The custom IPropertyNameGenerator implementation type in the form 'assemblyName:fullTypeName' or 'fullTypeName').")]
        public string PropertyNameGeneratorType { get; set; }

        [Argument(Name = "EnumNameGeneratorType", IsRequired = false, Description = "The custom IEnumNameGenerator implementation type in the form 'assemblyName:fullTypeName' or 'fullTypeName').")]
        public string EnumNameGeneratorType { get; set; }

        public void InitializeCustomTypes(AssemblyLoader.AssemblyLoader assemblyLoader)
        {
            if (!string.IsNullOrEmpty(TypeNameGeneratorType))
            {
                Settings.CodeGeneratorSettings.TypeNameGenerator = (ITypeNameGenerator)assemblyLoader.CreateInstance(TypeNameGeneratorType);
            }

            if (!string.IsNullOrEmpty(PropertyNameGeneratorType))
            {
                Settings.CodeGeneratorSettings.PropertyNameGenerator = (IPropertyNameGenerator)assemblyLoader.CreateInstance(PropertyNameGeneratorType);
            }

            if (!string.IsNullOrEmpty(EnumNameGeneratorType))
            {
                Settings.CodeGeneratorSettings.EnumNameGenerator = (IEnumNameGenerator)assemblyLoader.CreateInstance(EnumNameGeneratorType);
            }
        }
    }
}