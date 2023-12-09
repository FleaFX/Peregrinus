using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Peregrinus.Model.Exceptions;
using Semver;
using Xunit;

namespace Peregrinus.Model {
  public class RollbackEnabledApplicableMigrationTests {
    public class ConstructorTests : RollbackEnabledApplicableMigrationTests {
      [Fact]
      public void RequiresRollbackMigration() {
        Action act = () => new RollbackEnabledApplicableMigration(new SemVersion(2),
          new Description("Some description"), new MigrationScriptContent("CREATE SCHEMA [MySchema];"), null);

        act.Should().Throw<ArgumentNullException>();
      }

      [Theory]
      [InlineData(2, "Same Description", 1, "Same description")]
      [InlineData(2, "Same Description", 2, "Different description")]
      public void RequiresRollbackMigrationToApplyToCurrentMigration(
        int applicableMigrationVersion, string applicableMigrationDescription,
        int rollbackMigrationVersion, string rollbackMigrationDescription) {
        Action act = () => new RollbackEnabledApplicableMigration(
          new SemVersion(applicableMigrationVersion), new Description(applicableMigrationDescription), new MigrationScriptContent("CREATE SCHEMA [MySchema];"),
          new RollbackMigration(new SemVersion(rollbackMigrationVersion), new Description(rollbackMigrationDescription), new MigrationScriptContent("DROP SCHEMA [MySchema];")));

        act.Should().Throw<ArgumentOutOfRangeException>();
      }
    }

    public class ApplyTests : ApplicableMigrationTests {
      [Fact]
      public void PrereleaseVersionsCanBeApplied() {
        var migration = new RollbackEnabledApplicableMigration(
          new SemVersion(1,0,0,"alpha-1"),
          new Description("Some description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"),
          new RollbackMigration(
            new SemVersion(1,0,0,"alpha-1"),
            new Description("Some description"),
            new MigrationScriptContent("DROP SCHEMA [MySchema];")));

        Func<Task> act = () => migration.Apply(A.Dummy<IMigrationContext>());

        act.Should().NotThrowAsync<PermanentPrereleaseMigrationException>();
      }
    }
  }
}