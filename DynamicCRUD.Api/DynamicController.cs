using DynamicCRUD.Data;
using DynamicCRUD.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace DynamicCRUD.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class DynamicController : ControllerBase
    {
        DynamicDbContext _dynamicDbContext;
        public DynamicController(DynamicDbContext dynamicDbContext)
        {
            _dynamicDbContext = dynamicDbContext;
        }

        [HttpPost("GetCodelessEntity")]
        public async Task<ActionResult> GetCodelessEntity(JToken requestObject)
        {
            var metadataEntityName = requestObject["Entity"].ToString();
            var metadata = new MetadataEntity();  //Find and get your type from json 
            
                object response = null;
                var metadataQuerySet = (IQueryable<DynamicEntity>)_dynamicDbContext.GetType().GetMethod("Set").MakeGenericMethod(metadata.EntityType).Invoke(_dynamicDbContext, null);

                var filters = requestObject["Filters"].ToList();

                string selects = "*";

                if (requestObject["SelectList"] != null)
                    selects = string.Join(',', requestObject["SelectList"].ToList());

                var i = 0;
                foreach (var filter in filters)
                {
                    var filterValue = requestObject["FilterValues"].ElementAt(i).ToString();
                    i++;
                    metadataQuerySet = metadataQuerySet.Where(filter.ToString(), filterValue);
                }

                if (requestObject["SelectType"].ToString() == "List")
                    if (selects == "*")
                        response = await metadataQuerySet.ToDynamicListAsync();
                    else
                        response = await metadataQuerySet.Select($"new ({selects})").ToDynamicListAsync();

                if (requestObject["SelectType"].ToString() == "Single")
                    if (selects == "*")
                        response = await metadataQuerySet.FirstOrDefaultAsync();
                    else
                        response = await metadataQuerySet.Select($"new ({selects})").FirstOrDefault();


                return Ok(response);
        }

    }
}
