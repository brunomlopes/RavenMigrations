using System;
using Raven.Client;
using Raven.Client.Documents;
using RavenMigrations.Extensions;

namespace RavenMigrations.Migrations
{
    public abstract class Migration
    {
        public virtual void Down()
        {
        }

        public virtual void Setup(IDocumentStore documentStore)
        {
            DocumentStore = documentStore;
        }

        public abstract void Up();

        protected void WaitForIndexing()
        {
            throw new NotImplementedException("RavenDB5");
        }

        protected IDocumentStore DocumentStore { get; private set; }
    }
}