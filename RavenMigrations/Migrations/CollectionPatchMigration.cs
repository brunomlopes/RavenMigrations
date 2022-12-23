using System;
using Raven.Client.Documents.Indexes;

namespace RavenMigrations.Migrations
{
    public abstract class IndexPatchMigration<TIndex> : IndexPatchMigration 
        where TIndex : AbstractIndexCreationTask, new()
    {
        protected override string IndexName
        {
            get { return new TIndex().IndexName; }
        }
    }

    public class RavenDocumentsByEntityName : AbstractIndexCreationTask
    {
        public override IndexDefinition CreateIndexDefinition()
        {
            throw new NotImplementedException(
                "This is just a stub to be removed after implementing CollectionPatchMigration correctly");
        }
    }


    public abstract class CollectionPatchMigration<T> : IndexPatchMigration<RavenDocumentsByEntityName>
    {
        protected override string Query
        {
            get { return "Tag:" + DocumentStore.Conventions.GetCollectionName(typeof (T)); }
        }
    }
}