using System;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Peregrinus.Model.Exceptions;
using Semver;
using Xunit;

namespace Peregrinus.Model {
  public class MigrationsBatchTests {
    public class EqualsTests : MigrationsBatchTests {
      [Fact]
      public void BatchesContainingTheSameApplicableMigration_AreEqualInstances() {
        var batch1 = new MigrationsBatch(new ApplicableMigration(new SemVersion(1), new Description("A migration"), new MigrationScriptContent("CREATE SCHEMA [Test];")));
        var batch2 = new MigrationsBatch(new ApplicableMigration(new SemVersion(1), new Description("A migration"), new MigrationScriptContent("CREATE SCHEMA [Test];")));

        batch1.Should().Be(batch2);
      }

      [Fact]
      public void BatchesContainingDifferentApplicableMigrations_AreDifferentBatches() {
        var batch1 = new MigrationsBatch(new ApplicableMigration(new SemVersion(1), new Description("A migration"), new MigrationScriptContent("CREATE SCHEMA [Test];")));
        var batch2 = new MigrationsBatch(new ApplicableMigration(new SemVersion(2), new Description("Another migration"), new MigrationScriptContent("CREATE SCHEMA [Test2];")));

        batch1.Should().NotBe(batch2);
      }
    }

    public class GetHashCodeTests : MigrationsBatchTests {
      [Fact]
      public void BatchesContainingTheSameApplicableMigration_AreEqualHashCodes() {
        var batch1 = new MigrationsBatch(new ApplicableMigration(new SemVersion(1), new Description("A migration"), new MigrationScriptContent("CREATE SCHEMA [Test];")));
        var batch2 = new MigrationsBatch(new ApplicableMigration(new SemVersion(1), new Description("A migration"), new MigrationScriptContent("CREATE SCHEMA [Test];")));

        batch1.GetHashCode().Should().Be(batch2.GetHashCode());
      }

      [Fact]
      public void BatchesContainingDifferentApplicableMigrations_AreDifferentHashCodes() {
        var batch1 = new MigrationsBatch(new ApplicableMigration(new SemVersion(1), new Description("A migration"), new MigrationScriptContent("CREATE SCHEMA [Test];")));
        var batch2 = new MigrationsBatch(new ApplicableMigration(new SemVersion(2), new Description("Another migration"), new MigrationScriptContent("CREATE SCHEMA [Test2];")));

        batch1.GetHashCode().Should().NotBe(batch2.GetHashCode());
      }
    }

    public class CastFromApplicableMigrationTests : MigrationsBatchTests {
      [Fact]
      public void CastReturnsBatchWithSingleApplicableMigration() {
        var migration = new ApplicableMigration(new SemVersion(1), new Description("A migration"), new MigrationScriptContent("CREATE SCHEMA [Test];"));

        ((MigrationsBatch) migration).Should().Be(new MigrationsBatch(migration));
      }
    }

    public class CastFromRollbackMigrationTests : MigrationsBatchTests {
      [Fact]
      public void CastReturnsBatchWithSingleApplicableMigration() {
        var migration = new RollbackMigration(new SemVersion(1), new Description("A migration"), new MigrationScriptContent("DROP SCHEMA [Test];"));

        ((MigrationsBatch) migration).Should().Be(new MigrationsBatch(migration));
      }
    }

    public class PlusOperatorTests : MigrationsBatchTests {
      [Fact]
      public void AddingTwoBatchesReturnsBatchWithAllMigrationsFromBothBatches() {
        var migration1 = new ApplicableMigration(new SemVersion(1), new Description("A migration"), new MigrationScriptContent("CREATE SCHEMA [Test];"));
        var migration2 = new ApplicableMigration(new SemVersion(2), new Description("Another migration"), new MigrationScriptContent("CREATE SCHEMA [Test2];"));

        var batch1 = new MigrationsBatch(migration1);
        var batch2 = new MigrationsBatch(migration2);

        (batch1 + batch2).Should().Be(new MigrationsBatch(migration1, migration2));
      }

      [Fact]
      public void AddingTwoBatchesWithSameMigration_ReturnsNewBatchWithDistinctMigrations() {
        var migration1 = new ApplicableMigration(new SemVersion(1), new Description("A migration"), new MigrationScriptContent("CREATE SCHEMA [Test];"));
        var migration2 = new ApplicableMigration(new SemVersion(2), new Description("Another migration"), new MigrationScriptContent("CREATE SCHEMA [Test2];"));
        var migration3 = new ApplicableMigration(new SemVersion(3), new Description("Yet another migration"), new MigrationScriptContent("CREATE SCHEMA [Test3];"));

        var batch1 = new MigrationsBatch(migration1, migration2);
        var batch2 = new MigrationsBatch(migration2, migration3);

        (batch1 + batch2).Should().Be(new MigrationsBatch(migration1, migration2, migration3));
      }

      [Fact]
      public void AddingTwoBatchesWithApplicableAndRollbackMigrations_ReturnsReconciledRollbackEnabledApplicableMigration() {
        var applicableMigration = new ApplicableMigration(new SemVersion(1), new Description("A migration"), new MigrationScriptContent("CREATE SCHEMA [Test];"));
        var rollbackMigration = new RollbackMigration(new SemVersion(1), new Description("A migration"), new MigrationScriptContent("DROP SCHEMA [Test];"));

        var batch1 = (MigrationsBatch)applicableMigration;
        var batch2 = (MigrationsBatch)rollbackMigration;

        (batch1 + batch2).Should().Be((MigrationsBatch)new RollbackEnabledApplicableMigration(
          new SemVersion(1),
          new Description("A migration"),
          new MigrationScriptContent("CREATE SCHEMA [Test];"),
          rollbackMigration));
      }
    }

    public class ApplyToTests : MigrationsBatchTests {
      [Fact]
      public void BatchWithUnreconciledRollbackMigrationsCannotBeApplied() {
        var batch = (MigrationsBatch) new RollbackMigration(
          new SemVersion(2),
          new Description("Some migration"),
          new MigrationScriptContent("DROP TABLE [MyTable];")
        );

        Func<Task> act = () => batch.ApplyTo(A.Dummy<IMigrationHistory>(), res => { });

        act.Should().ThrowAsync<UnapplicableRollbackMigrationsException>();
      }

      [Fact]
      public async Task AppliesEachMigrationInSemverOrder() {
        var migrations = new[] {
          new ApplicableMigration(new SemVersion(3), new Description("Migration"), new MigrationScriptContent("GO;")),
          new ApplicableMigration(new SemVersion(2, 9), new Description("Migration"), new MigrationScriptContent("GO")),
          new ApplicableMigration(new SemVersion(4), new Description("Migration"), new MigrationScriptContent("GO")),
          new ApplicableMigration(new SemVersion(3, 0, 0, "alpha.1"), new Description("Migration"), new MigrationScriptContent("GO"))
        };
        var batch = new MigrationsBatch(migrations);

        var history = A.Fake<IMigrationHistory>();

        await batch.ApplyTo(history, res => { });

        A.CallTo(() => history.Apply(migrations[1])).MustHaveHappened()
          .Then(A.CallTo(() => history.Apply(migrations[3])).MustHaveHappened())
          .Then(A.CallTo(() => history.Apply(migrations[0])).MustHaveHappened())
          .Then(A.CallTo(() => history.Apply(migrations[2])).MustHaveHappened());
      }
    }

    public class MatchTests : MigrationsBatchTests {
      readonly MigrationsBatch _batch;

      public MatchTests() {
        _batch = new MigrationsBatch(
          new ApplicableMigration(
            new SemVersion(1),
            new Description("A migration"),
            new MigrationScriptContent("CREATE SCHEMA [MySchema];")
            )
          )
          +
          new RollbackEnabledApplicableMigration(
            new SemVersion(2),
            new Description("Another migration"),
            new MigrationScriptContent("CREATE TABLE [MyTable];"),
            new RollbackMigration(
              new SemVersion(2),
              new Description("Another migration"),
              new MigrationScriptContent("DROP TABLE [MyTable];")));
      }

      [Fact]
      public void RequiresBatchToBeFullyReconciled() {
        var batch = (MigrationsBatch)new RollbackMigration(
          new SemVersion(1),
          new Description("Unmatched rollback migration"),
          new MigrationScriptContent("DROP SCHEMA [MySchema];"));

        Func<Task> act = () => batch.Match(A.Dummy<AppliedMigration>());

        act.Should().ThrowAsync<InvalidOperationException>();
      }

      [Fact]
      public void RequiresGivenAppliedMigrationToHaveAMatchingApplicableMigration() {
        Func<Task> act = () => _batch.Match(
          new AppliedMigration(
            new SemVersion(1),
            new Description("A different initial migration"),
            new Checksum(new byte[] { 1, 2, 3, 4 })));

        act.Should().ThrowAsync<MigrationsBatchMismatchException>();
      }

      [Fact]
      public async Task WhenMatchingPermanentApplicableMigration_ThenReturnsGivenAppliedMigration() {
        var appliedMigration = new AppliedMigration(
          new SemVersion(1),
          new Description("A migration"),
          (Checksum)new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        var result = await _batch.Match(appliedMigration);

        result.Should().BeSameAs(appliedMigration);
      }

      [Fact]
      public async Task WhenMatchingRollbackEnabledApplicableMigration_ThenReturnsRollbackEnabledAppliedMigration() {
        var appliedMigration = new AppliedMigration(
          new SemVersion(2),
          new Description("Another migration"),
          (Checksum)new MigrationScriptContent("CREATE TABLE [MyTable];"));

        var result = await _batch.Match(appliedMigration);

        result.Should().Be(
          new RollbackEnabledAppliedMigration(
            new SemVersion(2),
            new Description("Another migration"),
            (Checksum) new MigrationScriptContent("CREATE TABLE [MyTable];"),
            new RollbackMigration(
              new SemVersion(2),
              new Description("Another migration"),
              new MigrationScriptContent("DROP TABLE [MyTable];")
            )
          )
        );
      }
    }
  }
}