using System;
using System.Threading.Tasks;
using FakeItEasy;
using Xunit;

namespace Peregrinus.Model {
  public class OrdinalRollbackStrategyTests {
    readonly IMigrationHistory _history;
    readonly RollbackStrategy _sut;

    public OrdinalRollbackStrategyTests() {
      _history = A.Fake<IMigrationHistory>();
      _sut = RollbackStrategy.Ordinal(5);
    }

    public class RollbackTests : OrdinalRollbackStrategyTests {
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

      [Fact]
      public void DoesNotRollbackMoreMigrationsThanTheRequestedCount() {
        A.CallTo(() => _history.Rollback(A<Predicate<AppliedMigration>>._)).ReturnsLazily(call => {
          var predicate = call.Arguments.Get<Predicate<AppliedMigration>>(0);
          return Task.FromResult(predicate(A.Dummy<AppliedMigration>()) ? (MigrationRollbackResult)
            MigrationRollbackResult.RollbackSingle.
              WithRolledBackMigration(A.Dummy<RollbackEnabledAppliedMigration>()).
              WithUpdatedHistory(_history) :
            MigrationRollbackResult.NoRollback.Because("All out, folks!")
          );
        });

        _sut.Rollback(_history);

        A.CallTo(() => _history.Rollback(A<Predicate<AppliedMigration>>._)).MustHaveHappened(5, Times.Exactly);
      }
    }
  }
}