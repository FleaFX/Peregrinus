using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Peregrinus.Model.Exceptions;
using Semver;
using Xunit;

namespace Peregrinus.Model {
  public class TargetVersionRollbackStrategyTests {
    readonly Stack<AppliedMigration> _appliedMigrations;
    readonly IMigrationHistory _history;
    RollbackStrategy _sut;

    public TargetVersionRollbackStrategyTests() {
      _appliedMigrations = new Stack<AppliedMigration>(new[] {
        new RollbackEnabledAppliedMigration(new SemVersion(1), new Description("A migration"), (Checksum)new MigrationScriptContent("CREATE TABLE [MyTable1];"), new RollbackMigration(new SemVersion(1), new Description("A migration"), new MigrationScriptContent("DROP TABLE [MyTable1];"))),
        new RollbackEnabledAppliedMigration(new SemVersion(2), new Description("A migration"), (Checksum)new MigrationScriptContent("CREATE TABLE [MyTable2];"), new RollbackMigration(new SemVersion(2), new Description("A migration"), new MigrationScriptContent("DROP TABLE [MyTable2];"))),
        new RollbackEnabledAppliedMigration(new SemVersion(3), new Description("A migration"), (Checksum)new MigrationScriptContent("CREATE TABLE [MyTable3];"), new RollbackMigration(new SemVersion(3), new Description("A migration"), new MigrationScriptContent("DROP TABLE [MyTable3];"))),
        new RollbackEnabledAppliedMigration(new SemVersion(4), new Description("A migration"), (Checksum)new MigrationScriptContent("CREATE TABLE [MyTable4];"), new RollbackMigration(new SemVersion(4), new Description("A migration"), new MigrationScriptContent("DROP TABLE [MyTable4];"))),
        new RollbackEnabledAppliedMigration(new SemVersion(5), new Description("A migration"), (Checksum)new MigrationScriptContent("CREATE TABLE [MyTable5];"), new RollbackMigration(new SemVersion(5), new Description("A migration"), new MigrationScriptContent("DROP TABLE [MyTable5];")))
      });

      _history = A.Fake<IMigrationHistory>();
      _sut = RollbackStrategy.TargetVersion(new SemVersion(2));

      A.CallTo(() => _history.Rollback(A<Predicate<AppliedMigration>>._)).ReturnsLazily(call => {
        var predicate = call.Arguments.Get<Predicate<AppliedMigration>>(0);
        return Task.FromResult(_appliedMigrations.TryPop(out var migration) && predicate(migration) ? (MigrationRollbackResult)
          MigrationRollbackResult.RollbackSingle.
            WithRolledBackMigration(migration as RollbackEnabledAppliedMigration).
            WithUpdatedHistory(_history) :
          MigrationRollbackResult.NoRollback.Because("All out, folks!")
        );
      });
    }

    public class RollbackTests : TargetVersionRollbackStrategyTests {
      [Fact]
      public void DoesNotRollbackBeyondRequestedVersion() {
        _sut.Rollback(_history);

        A.CallTo(() => _history.Rollback(A<Predicate<AppliedMigration>>._)).MustHaveHappened(4, Times.Exactly);
      }

      [Fact]
      public void RequiresRequestedVersionToBePresent() {
        _sut = RollbackStrategy.TargetVersion(new SemVersion(3, 5));

        Func<Task> act = () => _sut.Rollback(_history);
        
        act.Should().ThrowAsync<UnreachableRollbackTargetVersionException>();
      }
    }
  }
}