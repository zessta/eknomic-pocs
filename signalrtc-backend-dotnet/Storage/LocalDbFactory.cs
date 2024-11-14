using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SignalRtc.Storage
{
    public class LocalDbFactory
    {
        private static IServiceProvider _provider;
        public LocalDbFactory(IServiceProvider provider)
        {
            _provider = provider;
        }
        public IChatProvider GetLocalDb(string dbKey)
        {
            switch (dbKey)
            {
                case "Postgres":
                    return _provider.GetRequiredService<IChatPgStoreProvider>();
                case "RavenDB":
                    return _provider.GetRequiredService<IChatRavendbProvider>();
                case "PostgresNoSql":
                    return _provider.GetRequiredService<IChatPgJsonStoreProvider>();

                default:
                    break;
            }
            return null;
        }
    }
}
