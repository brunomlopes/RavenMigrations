using Raven.Client.Documents;
using RavenMigrations.Verbs;

namespace RavenMigrations.Migrations
{
    public abstract class Migration
    {
        protected Alter Alter { get; private set; }
        public virtual void Down()
        {
        }

        public virtual void Setup(IDocumentStore documentStore)
        {
            DocumentStore = documentStore;
            Alter = new Alter(documentStore);
        }

        public abstract void Up();

        protected IDocumentStore DocumentStore { get; private set; }
    }
}