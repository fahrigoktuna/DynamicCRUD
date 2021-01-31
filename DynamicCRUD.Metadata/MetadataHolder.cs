using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicCRUD.Metadata
{
    public class MetadataHolder
    {
        public List<MetadataEntity> Entities { get; set; } = new List<MetadataEntity>();

        public string Version { get; set; }
    }
}
