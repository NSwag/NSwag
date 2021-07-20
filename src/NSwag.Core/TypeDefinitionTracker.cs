using System;
using System.Collections.Generic;
using System.Text;

namespace NSwag
{
    public class TypeDefinitionTracker
    {
        public string GeneratedTypeName { get; set; }
        public string AttemptedTypeName { get; set; }
        public Type Type { get; set; }
    }
}
