using RavenMigrations.Migrations;

namespace RavenMigrations.Tests.Migrations.v1
{
    class M11_2_BaseTest<T> : CollectionPatchMigration<T>
    {
        public override string UpPatch { get; }
    }

    // string is nonsensical here, I know. It's just for testing purposes
    class M11_2_1_SixthBaseTest : M11_2_BaseTest<string> 
    {
    }
}