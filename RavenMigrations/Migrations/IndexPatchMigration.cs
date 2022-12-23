using System;
using Raven.Client;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Queries;

namespace RavenMigrations.Migrations
{
    public abstract class IndexPatchMigration : Migration
    {
        protected IndexPatchMigration()
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

        protected abstract string IndexName { get; }

        protected virtual IndexQuery IndexQuery
        {
            get
            {
                return new IndexQuery { Query = Query };
            }
        }

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
            throw new NotImplementedException("RAVENDB5: Not yet implemented correctly");

            DocumentStore
                .Operations
                .Send(new PatchByQueryOperation(new IndexQuery()
                {
                    //Query = $@"from index '{IndexName}'
                    //                   update {{
                    //                             {UpPatch}
                    //                   }}",
                    QueryParameters = UpPatchValues
                },
                    GetOperationOptions())).WaitForCompletion();
        }

        public override void Down()
        {
            if (string.IsNullOrWhiteSpace(DownPatch)) return;

            DocumentStore
                .Operations
                .Send(new PatchByQueryOperation(new IndexQuery()
                {
                    //Query = $@"from index '{IndexName}'
                    //                   update {{
                    //                             {DownPatch}
                    //                   }}",
                    QueryParameters = DownPatchValues
                },
                    GetOperationOptions())).WaitForCompletion();
        }
    }
}