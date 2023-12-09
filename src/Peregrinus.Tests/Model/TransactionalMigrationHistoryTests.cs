using System;
using System.Threading.Tasks;
using System.Transactions;
using FakeItEasy;
using FluentAssertions;
using Semver;
using Xunit;

namespace Peregrinus.Model {
  public class TransactionalMigrationHistoryTests {
    readonly IMigrationHistory _innerHistory;
    readonly TransactionalMigrationHistory _sut;

    protected TransactionalMigrationHistoryTests() {
      _innerHistory = A.Fake<IMigrationHistory>();
      _sut = new TransactionalMigrationHistory(_innerHistory);
    }

    public class ApplyTests : TransactionalMigrationHistoryTests {
      [Fact]
      public async Task DelegatesToInnerMigrationHistoryWithinTransaction() {
        (await DeferAssertion<bool>.For(async ranInTransaction => {
          var migration = new ApplicableMigration(new SemVersion(1), new Description("Initial database setup"),
            new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

          A.CallTo(() => _innerHistory.Apply(A<ApplicableMigration>._)).Invokes(call => { ranInTransaction.Satisfy(Transaction.Current != null); });

          await _sut.Apply(migration);
        })).Should().BeTrue();
      }

      [Fact]
      public async Task WhenInnerMigrationHistoryThrowsException_ThenReturnsApplicableMigrationFailedResult() {
        var migration = new ApplicableMigration(new SemVersion(1), new Description("Initial database setup"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));
        var thrownException = new Exception();
        A.CallTo(() => _innerHistory.Apply(A<ApplicableMigration>._)).Throws(thrownException);

        var result = await _sut.Apply(migration);

        result.Should().Be(new ApplicableMigrationFailedResult(migration, thrownException));
      }
    }

    public class RollbackTests : TransactionalMigrationHistoryTests {
      [Fact]
      public async Task DelegatesToInnerMigrationHistoryWithinTransaction() {
        (await DeferAssertion<bool>.For(async ranInTransaction => {
          A.CallTo(() => _innerHistory.Rollback((Predicate<AppliedMigration>) null)).Invokes(call => { ranInTransaction.Satisfy(Transaction.Current != null); });

          await _sut.Rollback();
        })).Should().BeTrue();
      }
    }

    public class StrategicRollbackTests : TransactionalMigrationHistoryTests {
      [Fact]
      public async Task DelegatesToInnerMigrationHistoryWithinTransaction() {
        (await DeferAssertion<bool>.For(async ranInTransaction => {
          var strategy = A.Dummy<RollbackStrategy>();
          A.CallTo(() => _innerHistory.Rollback(strategy)).Invokes(call => { ranInTransaction.Satisfy(Transaction.Current != null); });

          await _sut.Rollback(strategy);
        })).Should().BeTrue();
      }
    }
  }
}