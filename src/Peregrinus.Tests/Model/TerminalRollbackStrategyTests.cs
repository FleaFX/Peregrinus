using System;
using FakeItEasy;
using Xunit;

namespace Peregrinus.Model {
  public class TerminalRollbackStrategyTests {
    readonly IMigrationHistory _history;
    readonly RollbackStrategy _sut;

    public TerminalRollbackStrategyTests() {
      _history = A.Fake<IMigrationHistory>();
      _sut = RollbackStrategy.Terminal();
    }

    public class RollbackTests : TerminalRollbackStrategyTests {
      [Fact]
      public void DoesNotRollbackMoreMigrationsThanTheHistoryAllows() {
        A.CallTo(() => _history.Rollback(A<Predicate<AppliedMigration>>._)).ReturnsNextFromSequence(
          MigrationRollbackResult.RollbackSingle.
            WithRolledBackMigration(A.Dummy<RollbackEnabledAppliedMigration>()).
            WithUpdatedHistory(_history),

          MigrationRollbackResult.RollbackSingle.
            WithRolledBackMigration(A.Dummy<RollbackEnabledAppliedMigration>()).
            WithUpdatedHistory(_history),

          MigrationRollbackResult.RollbackSingle.
            WithRolledBackMigration(A.Dummy<RollbackEnabledAppliedMigration>()).
            WithUpdatedHistory(_history),

          MigrationRollbackResult.NoRollback.Because("All out, folks!")
        );

        _sut.Rollback(_history);

        A.CallTo(() => _history.Rollback(A<Predicate<AppliedMigration>>._)).MustHaveHappened(4, Times.Exactly);
      }
    }
  }
}