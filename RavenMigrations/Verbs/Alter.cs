using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Raven.Client.Documents;
using Raven.Client.Documents.Commands.Batches;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Session;
using Raven.Client.Json;
using Sparrow.Json.Parsing;

namespace RavenMigrations.Verbs
{
    public class Alter
    {
        public Alter(IDocumentStore documentStore)
        {
            DocumentStore = documentStore;
        }

        /// <summary>
        ///     Allows migration of a collection of documents one document at a time.
        /// </summary>
        /// <param name="tag">The name of the collection.</param>
        /// <param name="action">The action to migrate a single document and metadata.</param>
        /// <param name="pageSize">The page size for batching the documents.</param>
        public void Collection(string tag, Func<JObject, IMetadataDictionary, PatchRequest> action, int pageSize = 128)
        {
            SomeOfCollection(tag, (o, metadata) => (true, action(o, metadata)), pageSize);
        }

        /// <summary>
        ///     Allows migration of some documents of a collection of documents one document at a time.
        ///     Since saving a document might be vetoed or have side-effects (due to the versioning bundle), we want to be able to skip saving.
        /// </summary>
        /// <param name="tag">The name of the collection.</param>
        /// <param name="action">The action to migrate a single document and metadata.Returns whether we want to save it or not</param>
        /// <param name="pageSize">The page size for batching the documents.</param>
        public void SomeOfCollection(string tag, Func<JObject, IMetadataDictionary, (bool, PatchRequest)> action, int pageSize = 128)
        {
            using var session = DocumentStore.OpenSession(new SessionOptions {NoCaching = true, NoTracking = false});
            using var stream = session.Advanced.Stream(session.Advanced.RawQuery<JObject>($"from '{tag}'"));

            int entityCount = 0;

            while (stream.MoveNext())
            {
                var entity = stream.Current.Document;
                var metadata = stream.Current.Metadata;

                var (shouldPatch, patch) = action(entity, metadata);
                if (!shouldPatch) continue;

                session.Advanced.Defer(new PatchCommandData(
                    id: metadata["@id"].ToString(),
                    changeVector: null,
                    patch: patch,
                    patchIfMissing: null));

                if (entityCount >= pageSize)
                {
                    session.SaveChanges();
                    session.Advanced.Clear();
                }
            }
            session.SaveChanges();

        }

        public static void ExecuteCommands(IDocumentStore store, IList<ICommandData> patches)
        {
            var requestExecutor = store.GetRequestExecutor();
            using (requestExecutor.ContextPool.AllocateOperationContext(out var ctx))
            {
                var batchCommand = new SingleNodeBatchCommand(store.Conventions, ctx, patches);
                requestExecutor.Execute(batchCommand, ctx);
            }
        }

        protected IDocumentStore DocumentStore { get; private set; }
    }
}