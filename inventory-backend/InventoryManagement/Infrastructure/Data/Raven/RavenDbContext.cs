using Raven.Client.Documents.Session;
using Raven.Client.Documents;

namespace InventoryManagement.Infrastructure.Data.Raven
{
    public class RavenDbContext
    {
        private readonly IDocumentStore _store;

        public RavenDbContext(IDocumentStore store)
        {
            _store = store;
        }

        // Synchronous session
        public IDocumentSession Session => _store.OpenSession();

        // Asynchronous session
        public IAsyncDocumentSession AsyncSession => _store.OpenAsyncSession();
    }
}
