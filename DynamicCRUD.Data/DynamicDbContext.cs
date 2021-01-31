using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DynamicCRUD.Metadata;

namespace DynamicCRUD.Data
{
    public class DynamicDbContext : DbContext
    {


        string _version;
        public DynamicDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {
        }
        public List<MetadataEntity> _metaDataEntityList = new List<MetadataEntity>();

        public void AddMetadata(MetadataEntity metadataEntity) => _metaDataEntityList.Add(metadataEntity);

        public MetadataEntity GetMetadaEntity(Type type) => _metaDataEntityList.FirstOrDefault(p => p.EntityType == type);


        public void SetContextVersion(string version) => _version = version;

        public string GetContextVersion() => _version;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            foreach (var metadataEntity in _metaDataEntityList)
            {
                modelBuilder.Entity(metadataEntity.EntityType).ToTable(metadataEntity.TableName, metadataEntity.SchemaName).HasKey("Id");

                foreach (var metaDataEntityProp in metadataEntity.Properties)
                {
                    if (!metaDataEntityProp.IsNavigation)
                    {
                        var propBuilder = modelBuilder.Entity(metadataEntity.EntityType).Property(metaDataEntityProp.Name);

                        if (!string.IsNullOrEmpty(metaDataEntityProp.ColumnName))
                            propBuilder.HasColumnName(metaDataEntityProp.ColumnName);
                    }
                }
            }
            base.OnModelCreating(modelBuilder);
        }

    }

}
