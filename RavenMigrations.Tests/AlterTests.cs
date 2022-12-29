using System.Threading;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Raven.Client;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Session;
using Raven.TestDriver;
using RavenMigrations.Migrations;
using RavenMigrations.Verbs;
using Xunit;

namespace RavenMigrations.Tests
{
    public class AlterTests : RavenTestDriver
    {
        private IDocumentStore NewDocumentStore()
        {
            return GetDocumentStore();
        }

        [Fact]
        public void Can_migrate_down_from_new_clr_type()
        {
            using (var store = NewDocumentStore())
            {
                InitialiseWithPerson(store, "Sean Kearon");

                var migration = new AlterCollectionMigration();
                migration.Setup(store);

                migration.Up();
                WaitForIndexing(store);

                migration.Down();
                WaitForIndexing(store);

                using (var session = store.OpenSession())
                {
                    var customer = session.Load<Person1>("People/1");
                    customer.Name.Should().Be("Sean Kearon");
                }
            }
        }

        [Fact]
        public void Can_migrate_up_to_new_clr_type()
        {
            using (var store = NewDocumentStore())
            {
                InitialiseWithPerson(store, "Sean Kearon");

                var migration = new AlterCollectionMigration();
                migration.Setup(store);

                migration.Up();
                WaitForIndexing(store);

                Thread.SpinWait(100000000);

                using (var session = store.OpenSession())
                {
                    var customer = session.Load<Person2>("People/1");
                    customer.FirstName.Should().Be("Sean");
                    customer.LastName.Should().Be("Kearon");
                }
            }
        }

        private void InitialiseWithPerson(IDocumentStore store, string name)
        {
            using (var session = store.OpenSession())
            {
                session.Store(new Person1 { Id = "People/1", Name = name });
                session.SaveChanges();  
            }
            WaitForIndexing(store);
        }
    }

    public class AlterCollectionMigration : Migration
    {
        public override void Down()
        {
            Alter.Collection("Person1s", MigratePerson2ToPerson1);
        }

        public override void Up()
        {
            Alter.Collection("Person1s", MigratePerson1ToPerson2);
        }

        private PatchRequest MigratePerson2ToPerson1(JObject doc, IMetadataDictionary metadata)
        {
            var first = doc.Value<string>("FirstName");
            var last = doc.Value<string>("LastName");

            return new PatchRequest
            {
                Script = @"
this.Name = $Name;
this['@metadata'][$typeKey] = 'RavenMigrations.Tests.Person1, RavenMigrations.Tests'
",
                Values =
                {
                    {"Name", first + " " + last},
                    {"typeKey", Constants.Documents.Metadata.RavenClrType}
                }
            };
        }

        private PatchRequest MigratePerson1ToPerson2(JObject doc, IMetadataDictionary metadata)
        {
            var name = doc.Value<string>("Name");

            return new PatchRequest
            {
                Script = @"
this.FirstName = $FirstName;
this.LastName = $LastName;
delete this.Name;
this['@metadata'][$typeKey] = 'RavenMigrations.Tests.Person2, RavenMigrations.Tests'
",
                Values =
                {
                    {"FirstName", name?.Split(' ')[0]},
                    {"LastName", name?.Split(' ')[1]},
                    {"typeKey", Constants.Documents.Metadata.RavenClrType}
                }
            };

        }
    }

    public class Person1
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class Person2
    {
        public string FirstName { get; set; }
        public string Id { get; set; }
        public string LastName { get; set; }
    }
}