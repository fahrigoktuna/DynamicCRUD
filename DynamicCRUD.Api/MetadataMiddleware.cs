using DynamicCRUD.Data;
using DynamicCRUD.Emit;
using DynamicCRUD.Metadata;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace DynamicCRUD.Api
{
    public class MetadataMiddleware<TContext> where TContext : DynamicDbContext
    {
        private readonly RequestDelegate _next;

        public MetadataMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(
            HttpContext context,
            MetadataHolder metadataHolder,
            TContext dbContext)
        {
            var metadataHolderFromJson = new { Version = "", Entities = new List<MetadataEntity>() }; ///LOAD your metadata from cache   await _distributedCacheService.GetAsync<MetadataHolder>("MetadataJson");

            if (metadataHolderFromJson == null)
                throw new NullReferenceException("MetadataJson load failed.");

            var isNewMetadataJson = false;

            if (string.IsNullOrEmpty(metadataHolder.Version))
                isNewMetadataJson = true;
            else if (metadataHolder.Version != metadataHolderFromJson.Version)
                isNewMetadataJson = true;

            if (isNewMetadataJson)
            {
                Dictionary<string, TypeBuilder> entityTypeBuilderList = new Dictionary<string, TypeBuilder>();

                metadataHolder.Entities = new List<MetadataEntity>();

                var dynamicClassFactory = new DynamicClassFactory();
                foreach (var metadataEntity in metadataHolderFromJson.Entities)
                {
                    var metadataProps = new Dictionary<string, Type>();
                    foreach (var metaDataEntityProp in metadataEntity.Properties.Where(p => !p.IsNavigation))
                    {
                        // TODO YASIN datetime vb eklenebilir.
                        switch (metaDataEntityProp.Type)
                        {
                            case "String":
                                metadataProps.Add(metaDataEntityProp.Name, typeof(string));
                                break;
                            case "Int":
                                metadataProps.Add(metaDataEntityProp.Name, typeof(int));
                                break;
                            case "Guid":
                                metadataProps.Add(metaDataEntityProp.Name, typeof(Guid));
                                break;
                            default:
                                //Implement for Other types
                                break;
                        }
                    }
                    if (string.IsNullOrEmpty(metadataEntity.CustomAssemblyType))
                    {
                        var entityTypeBuilder = dynamicClassFactory.CreateDynamicTypeBuilder<DynamicEntity>(metadataEntity.Name, metadataProps);

                        entityTypeBuilderList.Add(metadataEntity.Name, entityTypeBuilder);
                    }

                    else
                        metadataEntity.EntityType = Type.GetType(metadataEntity.CustomAssemblyType);


                    metadataHolder.Entities.Add(metadataEntity);
                }

                metadataHolder.Version = metadataHolderFromJson.Version;

                foreach (var metadataEntity in metadataHolder.Entities)
                {
                    var existEntityTypeBuilder = entityTypeBuilderList.FirstOrDefault(p => p.Key == metadataEntity.Name).Value;

                    foreach (var metaDataEntityProp in metadataEntity.Properties.Where(p => p.IsNavigation))
                    {
                        var relatedEntityTypeBuilder = entityTypeBuilderList.FirstOrDefault(p => p.Key == metaDataEntityProp.Type).Value;

                        var relatedProp = new Dictionary<string, Type>();

                        if (metaDataEntityProp.NavigationType == "Single")
                            relatedProp.Add(metaDataEntityProp.Name, relatedEntityTypeBuilder);
                        else
                        {
                            Type listGenericType = typeof(List<>);

                            Type listRelateEntityType = listGenericType.MakeGenericType(relatedEntityTypeBuilder);

                            relatedProp.Add(metaDataEntityProp.Name, listRelateEntityType);
                        }
                        new DynamicClassFactory().CreatePropertiesForTypeBuilder(existEntityTypeBuilder, relatedProp, null);
                    }
                    metadataEntity.EntityType = existEntityTypeBuilder.CreateType();
                }
            }

            foreach (var metaDataEntity in metadataHolder.Entities)
                dbContext.AddMetadata(metaDataEntity);

            dbContext.SetContextVersion(metadataHolder.Version);

            await _next.Invoke(context);
        }
    }

    public static class DynamicContextMiddlewareExtensions
    {
        public static IApplicationBuilder UseDynamicContext<TContext>(this IApplicationBuilder builder) where TContext : DynamicDbContext
        {
            return builder.UseMiddleware<MetadataMiddleware<TContext>>();
        }
    }
}
