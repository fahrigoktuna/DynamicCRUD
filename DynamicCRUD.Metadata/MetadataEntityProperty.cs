using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicCRUD.Metadata
{
    public class MetadataEntityProperty
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string ColumnName { get; set; }

        public bool IsNavigation { get; set; }

        public string NavigationType { get; set; } = "Single";
    }
}
