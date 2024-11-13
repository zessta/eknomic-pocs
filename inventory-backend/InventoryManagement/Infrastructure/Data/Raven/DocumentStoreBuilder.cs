using Raven.Client.Documents.Operations;
using Raven.Client.Documents;
using Raven.Client.Exceptions.Database;
using Raven.Client.ServerWide.Operations;
using Raven.Client.ServerWide;
using System.Security.Cryptography.X509Certificates;

namespace InventoryManagement.Infrastructure.Data.Raven
{
    public class DocumentStoreBuilder
    {
        public static IDocumentStore Initialize()
        {
            var store = new DocumentStore
            {
                Urls = new[] { "https://a.zcoder.ravendb.community" },
                Database = "Inventory",
                Certificate = new X509Certificate2(@"C:\Users\ADMIN\Downloads\client-certificate\client-certificate.pfx", "")
            };

            store.Initialize();

            EnsureDBIsCreated(store);

            return store;
        }

        private static void EnsureDBIsCreated(DocumentStore store)
        {
            try
            {
                store.Maintenance.ForDatabase(store.Database).Send(new GetStatisticsOperation());
            }
            catch (DatabaseDoesNotExistException)
            {
                store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(store.Database)));
            }
        }
    }
}
