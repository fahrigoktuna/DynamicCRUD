using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DynamicCRUD.Data
{
    public class CustomModelCacheKey : ModelCacheKey
    {
        readonly string _contextVersion;
        public CustomModelCacheKey(DbContext context)
            : base(context)
        {
            _contextVersion = (context as DynamicDbContext)?.GetContextVersion();
        }

        protected override bool Equals(ModelCacheKey other)
         => base.Equals(other)
            && (other as CustomModelCacheKey)?._contextVersion == _contextVersion;

        public override int GetHashCode() => base.GetHashCode();
    }
}
