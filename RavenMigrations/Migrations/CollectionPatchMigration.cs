using Raven.Client.Documents.Indexes;

namespace RavenMigrations.Migrations
{
    public abstract class IndexPatchMigration:PatchMigration
    {
        protected abstract string IndexName { get; }

        protected override string GetIndexPartForQuery()
        {
            return $"index '{IndexName}'";
        }


    }
    public abstract class IndexPatchMigration<TIndex> : IndexPatchMigration 
        where TIndex : AbstractIndexCreationTask, new()
    {
        protected override string IndexName
        {
            get { return new TIndex().IndexName; }
        }
    }


    public abstract class CollectionPatchMigration<T> : PatchMigration
    {
        protected override string GetIndexPartForQuery()
        {
            return DocumentStore.Conventions.GetCollectionName(typeof(T));
        }

        // On raven 3 we would filter on Tag:, but now on 5 we filter on the `from` clause
        protected override string Query => "";
    }
}