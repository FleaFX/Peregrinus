using System;
using FluentAssertions;
using Semver;
using Xunit;

namespace Peregrinus.Model {
  public class RollbackEnabledAppliedMigrationTests {
    public class ConstructorTests : RollbackEnabledAppliedMigrationTests {
      [Fact]
      public void RequiresRollbackMigration() {
        Action act = () => new RollbackEnabledAppliedMigration(new SemVersion(2),
          new Description("Some description"), new Checksum(new byte[] { 1, 2, 3, 4 }), null);

        act.Should().Throw<ArgumentNullException>();
      }

      [Theory]
      [InlineData(2, "Same Description", 1, "Same description")]
      [InlineData(2, "Same Description", 2, "Different description")]
      public void RequiresRollbackMigrationToApplyToCurrentMigration(
        int applicableMigrationVersion, string applicableMigrationDescription,
        int rollbackMigrationVersion, string rollbackMigrationDescription) {
        Action act = () => new RollbackEnabledAppliedMigration(
          new SemVersion(applicableMigrationVersion), new Description(applicableMigrationDescription), new Checksum(new byte[] { 1, 2, 3, 4 }),
          new RollbackMigration(new SemVersion(rollbackMigrationVersion), new Description(rollbackMigrationDescription), new MigrationScriptContent("DROP SCHEMA [MySchema];")));

        act.Should().Throw<ArgumentOutOfRangeException>();
      }
    }
    
  }
}