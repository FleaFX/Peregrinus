using FakeItEasy;
using Peregrinus.Model;
using Semver;

namespace Peregrinus; 

public class RollbackMigrationDummyFactory : DummyFactory<RollbackEnabledAppliedMigration> {
    protected override RollbackEnabledAppliedMigration Create() =>
        new(new SemVersion(1), Description.None, new Checksum(new byte []{ 0, 1, 2, 3, 4}),
            new RollbackMigration(new SemVersion(1), Description.None, new MigrationScriptContent("")));
}