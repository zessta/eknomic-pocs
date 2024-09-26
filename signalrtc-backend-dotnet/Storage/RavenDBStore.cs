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
                    Urls = new[] { "http://localhost:8080" }, // Your RavenDB URL
                    Database = "ChatAppDb" // Name of your database
                }.Initialize();
            }

            return store;
        }
    }

}
