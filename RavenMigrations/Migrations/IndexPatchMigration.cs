using System;
using System.Linq;
using Raven.Client;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Queries;

namespace RavenMigrations.Migrations
{
    public abstract class PatchMigration : Migration
    {
        protected PatchMigration()
        {
            IndexingTimeout = TimeSpan.FromMinutes(5);
        }

        public abstract string UpPatch { get; }

        public virtual Parameters UpPatchValues
        {
            get { return new Parameters(); }
        }
        public virtual string DownPatch { get { return null; } }
        public virtual Parameters DownPatchValues
        {
            get { return new Parameters(); }
        }
        
        protected abstract string GetIndexPartForQuery();

        protected TimeSpan IndexingTimeout { get; set; }

        protected virtual QueryOperationOptions GetOperationOptions()
        {
            return new QueryOperationOptions
            {
                StaleTimeout = IndexingTimeout
            };
        }

        protected abstract string Query { get; }

        public override void Up()
        {
            DocumentStore.Operations.Send(new PatchByQueryOperation(
                new IndexQuery
                {
                    Query = GetUpdateIndexQuery(UpPatch),
                    QueryParameters = UpPatchValues,
                }, GetOperationOptions())).WaitForCompletion();
        }

        public override void Down()
        {
            if (string.IsNullOrWhiteSpace(DownPatch)) return;

            DocumentStore
                .Operations
                .Send(new PatchByQueryOperation(new IndexQuery()
                {
                    Query = GetUpdateIndexQuery(DownPatch),
                    QueryParameters = DownPatchValues
                },
                    GetOperationOptions())).WaitForCompletion();
        }

        protected string GetUpdateIndexQuery(string patch)
        {
            var indexPartForQuery = GetIndexPartForQuery();
            var updateQuery = $"from {indexPartForQuery}\n";
            if (!string.IsNullOrWhiteSpace(Query))
            {
                // This is a shim to support lucene queries. since 'lucene' requires an indexed field name
                // we can guess that any field on the query would be indexed.
                // the field would be the first part of (name:Ali*), split by :
                var splitResult = Query.TrimStart('(', ' ').Split(new char[]{':'}, 2);
                var field = splitResult.First();
                var value = splitResult.Last();

                updateQuery += $"WHERE search({field}, \"{value}\")\n";
            }

            updateQuery += $"update {{ {patch} }}";
            return updateQuery;
        }
    }
}