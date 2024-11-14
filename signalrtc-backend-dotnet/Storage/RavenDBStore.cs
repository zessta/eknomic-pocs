using Raven.Client.Documents;

namespace SignalRtc.Storage
{
    public static class RavenDbStore
    {
        private static IDocumentStore store;

        public static IDocumentStore GetStore()
        {
            if (store == null)
            {
                store = new DocumentStore
                {
                    //Urls = new[] { "http://120.0.0.1:8080" }, // RavenDB URL
                    Urls = new[] { "http://localhost:8080" }, // RavenDB URL
                    Database = "ChatAppDb" // Name of database
                }.Initialize();
            }

            return store;
        }
    }

}
