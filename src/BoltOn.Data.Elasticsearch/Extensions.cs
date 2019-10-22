using System.Reflection;
using BoltOn.Bootstrapping;

namespace BoltOn.Data.Elasticsearch
{
    public static class Extensions
    {
        public static BoltOnOptions BoltOnElasticsearchModule(this BoltOnOptions boltOnOptions)
        {
            boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
            return boltOnOptions;
        }
    }
}
