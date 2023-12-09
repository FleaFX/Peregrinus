using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Peregrinus.Database;
using Semver;
using Xunit;

namespace Peregrinus.Model {
  public class MigrationHistoryTests {
    readonly IQueryExecutor _queryExecutor;
    readonly IMigrationExecutor _migrationExecutor;
    readonly MigrationContext _migrationContext;
    IEnumerable<AppliedMigration> _appliedMigrations;
    MigrationHistory _sut;

    protected MigrationHistoryTests() {
      _queryExecutor = A.Fake<IQueryExecutor>();
      _migrationExecutor = A.Dummy<IMigrationExecutor>();
      _migrationContext = new MigrationContext(_queryExecutor, _migrationExecutor, "MigrationsTest");
      //A.CallTo(() => _queryExecutor.NewQuery(A<string>._)).Returns(_queryExecutor);
      A.CallTo(_queryExecutor).WithReturnType<IQueryExecutor>().Returns(_queryExecutor);
      A.CallTo(_migrationExecutor).WithReturnType<IMigrationExecutor>().Returns(_migrationExecutor);

      _appliedMigrations = new[] {
        new AppliedMigration(new SemVersion(1), new Description("Initial migration"),
          new Checksum(Convert.FromBase64String("/sYW2qnA+8nQZ9cbQLFqAo+na5k="))),
        new AppliedMigration(new SemVersion(2), new Description("Add some table"), 
          new Checksum(Convert.FromBase64String("JNXnwpnsd9KkOn/cO/MGZVNsbcE=")))
      };
      _sut = new MigrationHistory(_migrationContext, _appliedMigrations.ToArray());
    }

    public class ApplyTests : MigrationHistoryTests {

      [Fact]
      public async Task WhenMigrationIsAlreadyApplied_ReturnsAppliedMigrationSkippedResult() {
        var migration = new ApplicableMigration(new SemVersion(1), new Description("Initial migration"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        var result = await _sut.Apply(migration);

        result.Should().Be(new AppliedMigrationSkippedResult(migration, _appliedMigrations.First()));
      }

      [Theory]
      [InlineData(1, 5, 0)] // earlier
      [InlineData(2, 0, 0)] // equal
      public async Task WhenMigrationVersionIsEarlierThanOrEqualToLastAppliedVersion_ReturnsMigrationVersionAnachronismResult(int major, int minor, int patch) {
        var migration = new ApplicableMigration(new SemVersion(major, minor, patch), new Description("Create a new table"),
          new MigrationScriptContent("CREATE TABLE [MyTable] ([Id] [int] IDENTITY(1,1) NOT NULL)"));

        var result = await _sut.Apply(migration);

        result.Should().Be(new MigrationVersionAnachronismResult(migration, new SemVersion(2)));
      }

      [Fact]
      public async Task WhenMigrationIsApplicable_ExecutesTheQueryOfTheMigration() {
        var migration = new ApplicableMigration(new SemVersion(3), new Description("Create a new table"),
          new MigrationScriptContent("CREATE TABLE [MyTable] ([Id] [int] IDENTITY(1,1) NOT NULL)"));

        await _sut.Apply(migration);

        A.CallTo(() => _migrationExecutor.NewMigration("CREATE TABLE [MyTable] ([Id] [int] IDENTITY(1,1) NOT NULL)")).MustHaveHappened();
        A.CallTo(() => _migrationExecutor.ExecuteAsync()).MustHaveHappened();
      }

      [Fact]
      public async Task WhenMigrationIsApplicable_WritesAHistoryRecordForTheMigration() {
        var migration = new ApplicableMigration(new SemVersion(3), new Description("Create a new table"),
          new MigrationScriptContent("CREATE TABLE [MyTable] ([Id] [int] IDENTITY(1,1) NOT NULL)"));

        await _sut.Apply(migration);

        A.CallTo(() => _queryExecutor.NewQuery(@"
INSERT INTO [MigrationsTest].[dbo].[migration_history] ([Id], [TimeStamp], [Version], [Description], [Checksum], [ExecutionTime])
VALUES (NEWID(), @TimeStampTicks, @Version, @Description, @Checksum, @ExecutionTimeTicks)
")).MustHaveHappened();
        A.CallTo(() => _queryExecutor.ExecuteAsync()).MustHaveHappened();
      }

      [Fact]
      public async Task WhenMigrationIsApplicable_ReturnsAppliedMigrationResult() {
        var migration = new ApplicableMigration(new SemVersion(3), new Description("Create a new table"),
          new MigrationScriptContent("CREATE TABLE [MyTable] ([Id] [int] IDENTITY(1,1) NOT NULL)"));

        var result = await _sut.Apply(migration);

        result.Should().Be(new ApplicableMigrationSucceededResult(migration, await migration.Apply(_migrationContext), new MigrationHistory(_migrationContext, _appliedMigrations.Concat(new [] { await migration.Apply(_migrationContext) }).ToArray())));
      }

      [Fact]
      public async Task WhenApplyingMigrationTwice_ReturnsSuccessThenSkipped() {
        var migration = new ApplicableMigration(new SemVersion(3), new Description("Create a new table"),
          new MigrationScriptContent("CREATE TABLE [MyTable] ([Id] [int] IDENTITY(1,1) NOT NULL)"));

        var firstResult = await _sut.Apply(migration);
        firstResult.Should().Be(new ApplicableMigrationSucceededResult(migration, await migration.Apply(_migrationContext), new MigrationHistory(_migrationContext, _appliedMigrations.Concat(new [] { await migration.Apply(_migrationContext) }).ToArray())));

        var secondResult = await firstResult.As<ApplicableMigrationSucceededResult>().UpdatedHistory.Apply(migration);
        secondResult.Should().Be(new AppliedMigrationSkippedResult(migration, ((ApplicableMigrationSucceededResult)firstResult).AppliedMigration));
      }

      [Fact]
      public async Task WhenLastMigrationWasPrerelease_RollsBackLastMigration() {
        _appliedMigrations = new[] {
          new AppliedMigration(new SemVersion(1), new Description("Initial migration"),
            new Checksum(Convert.FromBase64String("/sYW2qnA+8nQZ9cbQLFqAo+na5k="))),
          new RollbackEnabledAppliedMigration(new SemVersion(2, 0, 0, "alpha.1"), new Description("Add some table"), 
            new Checksum(Convert.FromBase64String("JNXnwpnsd9KkOn/cO/MGZVNsbcE=")),
            new RollbackMigration(new SemVersion(2, 0, 0, "alpha.1"), new Description("Add some table"),
              new MigrationScriptContent("DROP TABLE [SomeTable];")))
        };
        _sut = new MigrationHistory(_migrationContext, _appliedMigrations.ToArray());

        var migration = new ApplicableMigration(new SemVersion(3), new Description("Create a new table"),
          new MigrationScriptContent("CREATE TABLE [MyTable] ([Id] [int] IDENTITY(1,1) NOT NULL)"));

        await _sut.Apply(migration);

        A.CallTo(() => _migrationExecutor.NewMigration("DROP TABLE [SomeTable];")).MustHaveHappened();
        A.CallTo(() => _migrationExecutor.ExecuteAsync()).MustHaveHappened();
      }

      [Fact]
      public async Task WhenLastMigrationWasPrerelease_RemovesTheHistoryRecordForTheLastMigration() {
        _appliedMigrations = new[] {
          new AppliedMigration(new SemVersion(1), new Description("Initial migration"),
            new Checksum(Convert.FromBase64String("/sYW2qnA+8nQZ9cbQLFqAo+na5k="))),
          new RollbackEnabledAppliedMigration(new SemVersion(2, 0, 0, "alpha.1"), new Description("Add some table"), 
            new Checksum(Convert.FromBase64String("JNXnwpnsd9KkOn/cO/MGZVNsbcE=")),
            new RollbackMigration(new SemVersion(2, 0, 0, "alpha.1"), new Description("Add some table"),
              new MigrationScriptContent("DROP TABLE [SomeTable];")))
        };
        _sut = new MigrationHistory(_migrationContext, _appliedMigrations.ToArray());

        var migration = new ApplicableMigration(new SemVersion(3), new Description("Create a new table"),
          new MigrationScriptContent("CREATE TABLE [MyTable] ([Id] [int] IDENTITY(1,1) NOT NULL)"));

        await _sut.Apply(migration);

        A.CallTo(() => _queryExecutor.NewQuery(@"
DELETE FROM [MigrationsTest].[dbo].[migration_history]
WHERE [Version] = @Version AND [Description] = @Description
")).MustHaveHappened();
        A.CallTo(() => _queryExecutor.ExecuteAsync()).MustHaveHappened();
      }

      [Fact]
      public async Task WhenLastMigrationWasPrerelease_ReturnsAppliedMigrationWithRollbackResult() {
        _appliedMigrations = new[] {
          new AppliedMigration(new SemVersion(1), new Description("Initial migration"),
            new Checksum(Convert.FromBase64String("/sYW2qnA+8nQZ9cbQLFqAo+na5k="))),
          new RollbackEnabledAppliedMigration(new SemVersion(2, 0, 0, "alpha.1"), new Description("Add some table"), 
            new Checksum(Convert.FromBase64String("JNXnwpnsd9KkOn/cO/MGZVNsbcE=")),
            new RollbackMigration(new SemVersion(2, 0, 0, "alpha.1"), new Description("Add some table"),
              new MigrationScriptContent("DROP TABLE [SomeTable];")))
        };
        _sut = new MigrationHistory(_migrationContext, _appliedMigrations.ToArray());

        var migration = new ApplicableMigration(new SemVersion(3), new Description("Create a new table"),
          new MigrationScriptContent("CREATE TABLE [MyTable] ([Id] [int] IDENTITY(1,1) NOT NULL)"));

        var result = await _sut.Apply(migration);

        result.Should().Be(new ApplicableMigrationSucceededWithRollbackResult(migration, await migration.Apply(_migrationContext), _appliedMigrations.Last() as RollbackEnabledAppliedMigration, new MigrationHistory(_migrationContext, _appliedMigrations.Take(1).Concat(new [] { await migration.Apply(_migrationContext) }).ToArray())));
      }
    }

    public class RollbackTests : MigrationHistoryTests {
      [Fact]
      public async Task GivenHistoryContainsNoSteps_ReturnsNoRollbackResult() {
        var history = new MigrationHistory(_migrationContext);

        var result = await history.Rollback();

        result.Should().BeOfType<NoRollbackResult>();
      }

      [Fact]
      public async Task GivenLastMigrationHasNoRollbackCapacity_ReturnsNoRollbackResult() {
        var result = await _sut.Rollback();

        result.Should().BeOfType<NoRollbackResult>();
      }

      [Fact]
      public async Task GivenPredicateReturnsFalse_ReturnsNoRollbackResult() {
        var history = new MigrationHistory(_migrationContext,
          new RollbackEnabledAppliedMigration(
            new SemVersion(1),
            new Description("A migration"),
            (Checksum)new MigrationScriptContent("CREATE SCHEMA[MySchema];"),
            new RollbackMigration(
              new SemVersion(1),
              new Description("A migration"),
              new MigrationScriptContent("DROP SCHEMA [MySchema];"))));

        var result = await history.Rollback(_ => false);

        result.Should().BeOfType<NoRollbackResult>();
      }

      [Fact]
      public async Task GivenNoPredicate_PerformsTheRollbackScript() {
        var history = new MigrationHistory(_migrationContext,
          new RollbackEnabledAppliedMigration(
            new SemVersion(1),
            new Description("A migration"),
            (Checksum)new MigrationScriptContent("CREATE SCHEMA[MySchema];"),
            new RollbackMigration(
              new SemVersion(1),
              new Description("A migration"),
              new MigrationScriptContent("DROP SCHEMA [MySchema];"))));

        await history.Rollback();

        A.CallTo(() => _migrationExecutor.NewMigration("DROP SCHEMA [MySchema];")).MustHaveHappened();
        A.CallTo(() => _migrationExecutor.ExecuteAsync()).MustHaveHappened();
      }

      [Fact]
      public async Task GivenNoPredicate_RemovesTheHistoryRecord() {
        var history = new MigrationHistory(_migrationContext,
          new RollbackEnabledAppliedMigration(
            new SemVersion(1),
            new Description("A migration"),
            (Checksum)new MigrationScriptContent("CREATE SCHEMA[MySchema];"),
            new RollbackMigration(
              new SemVersion(1),
              new Description("A migration"),
              new MigrationScriptContent("DROP SCHEMA [MySchema];"))));

        await history.Rollback();

        A.CallTo(() => _queryExecutor.NewQuery(@"
DELETE FROM [MigrationsTest].[dbo].[migration_history]
WHERE [Version] = @Version AND [Description] = @Description
")).MustHaveHappened();
        A.CallTo(() => _queryExecutor.ExecuteAsync()).MustHaveHappened();
      }

      [Fact]
      public async Task GivenNoPredicate_ReturnsRollbackSingleResult() {
        var appliedMigrations = new AppliedMigration[] {
          new RollbackEnabledAppliedMigration(
            new SemVersion(1),
            new Description("A migration"),
            (Checksum)new MigrationScriptContent("CREATE SCHEMA[MySchema];"),
            new RollbackMigration(
              new SemVersion(1),
              new Description("A migration"),
              new MigrationScriptContent("DROP SCHEMA [MySchema];")))
        };
        var history = new MigrationHistory(_migrationContext, appliedMigrations);

        var result = await history.Rollback();

        result.Should().Be(new RollbackSingleResult(appliedMigrations[0] as RollbackEnabledAppliedMigration, new MigrationHistory(_migrationContext)));
      }

      [Fact]
      public async Task GivenPredicateReturnsTrue_PerformsTheRollbackScript() {
        var history = new MigrationHistory(_migrationContext,
          new RollbackEnabledAppliedMigration(
            new SemVersion(1),
            new Description("A migration"),
            (Checksum)new MigrationScriptContent("CREATE SCHEMA[MySchema];"),
            new RollbackMigration(
              new SemVersion(1),
              new Description("A migration"),
              new MigrationScriptContent("DROP SCHEMA [MySchema];"))));

        await history.Rollback();

        A.CallTo(() => _migrationExecutor.NewMigration("DROP SCHEMA [MySchema];")).MustHaveHappened();
        A.CallTo(() => _migrationExecutor.ExecuteAsync()).MustHaveHappened();
      }

      [Fact]
      public async Task GivenPredicateReturnsTrue_RemovesTheHistoryRecord() {
        var history = new MigrationHistory(_migrationContext,
          new RollbackEnabledAppliedMigration(
            new SemVersion(1),
            new Description("A migration"),
            (Checksum)new MigrationScriptContent("CREATE SCHEMA[MySchema];"),
            new RollbackMigration(
              new SemVersion(1),
              new Description("A migration"),
              new MigrationScriptContent("DROP SCHEMA [MySchema];"))));

        await history.Rollback();

        A.CallTo(() => _queryExecutor.NewQuery(@"
DELETE FROM [MigrationsTest].[dbo].[migration_history]
WHERE [Version] = @Version AND [Description] = @Description
")).MustHaveHappened();
        A.CallTo(() => _queryExecutor.ExecuteAsync()).MustHaveHappened();
      }

      [Fact]
      public async Task GivenPredicateReturnsTrue_ReturnsRollbackSingleResult() {
        var appliedMigrations = new AppliedMigration[] {
          new RollbackEnabledAppliedMigration(
            new SemVersion(1),
            new Description("A migration"),
            (Checksum)new MigrationScriptContent("CREATE SCHEMA[MySchema];"),
            new RollbackMigration(
              new SemVersion(1),
              new Description("A migration"),
              new MigrationScriptContent("DROP SCHEMA [MySchema];")))
        };
        var history = new MigrationHistory(_migrationContext, appliedMigrations);

        var result = await history.Rollback();

        result.Should().Be(new RollbackSingleResult(appliedMigrations[0] as RollbackEnabledAppliedMigration, new MigrationHistory(_migrationContext)));
      }
    }

    public class StrategicRollbackTests : MigrationHistoryTests {
      [Fact]
      public async Task EmploysGivenStrategyToPerformRollback() {
        var strategy = A.Fake<RollbackStrategy>();

        await _sut.Rollback(strategy);

        A.CallTo(() => strategy.Rollback(_sut)).MustHaveHappened();
      }

      [Fact]
      public async Task ReturnsRollbackCompleteResult() {
        var strategy = A.Dummy<RollbackStrategy>();
        var resultHistory = A.Dummy<IMigrationHistory>();
        A.CallTo(() => strategy.Rollback(_sut)).Returns(resultHistory);

        var result = await _sut.Rollback(strategy);

        result.Should().BeOfType<RollbackByStrategyResult>();
        result.As<RollbackByStrategyResult>().UpdatedHistory.Should().Be(resultHistory);
      }
    }
  }
}