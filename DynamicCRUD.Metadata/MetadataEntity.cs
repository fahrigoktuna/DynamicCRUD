using System;
using System.Collections.Generic;

namespace DynamicCRUD.Metadata
{
    public class MetadataEntity
    {
        public string Name { get; set; }

        public string TableName { get; set; }

        public string SchemaName { get; set; }

        public Type EntityType { get; set; } 

        public List<MetadataEntityProperty> Properties { get; set; }

        public string CustomAssemblyType { get; set; }

        public string CustomServiceAssemblyType { get; set; }

        public bool CustomSelect { get; set; }
    }
}
