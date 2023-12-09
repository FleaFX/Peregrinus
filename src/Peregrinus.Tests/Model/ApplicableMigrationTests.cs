using System;
using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Peregrinus.Model.Exceptions;
using Semver;
using Xunit;

namespace Peregrinus.Model {
  public class ApplicableMigrationTests {

    public class FromFileTests : ApplicableMigrationTests { 
      [Fact]
      public void RequiresName() {
        Action act = () => ApplicableMigration.FromStream(null, A.Dummy<Stream>());
        act.Should().Throw<ArgumentNullException>();
      }

      [Fact]
      public void RequiresStream() {
        Action act = () => ApplicableMigration.FromStream("V1.0.0__Migration.sql", null);
        act.Should().Throw<ArgumentNullException>();
      }

      [Fact]
      public void RequiresCorrectPrefixInFilename() {
        Action act = () => ApplicableMigration.FromStream("R1.0.0__NotAnApplicableMigration.sql", A.Dummy<Stream>());
        act.Should().Throw<ArgumentOutOfRangeException>();
      }

      [Fact]
      public void RequiresValidSemverVersionInFilename() {
        Action act = () => ApplicableMigration.FromStream("V1__InvalidSemverVersion.sql", A.Dummy<Stream>());
        act.Should().Throw<ArgumentOutOfRangeException>();
      }

      [Fact]
      public void RequiresDescriptionInFilename() {
        Action act = () => ApplicableMigration.FromStream("V1.0.0__.sql", A.Dummy<Stream>());
        act.Should().Throw<ArgumentOutOfRangeException>();
      }

      [Fact]
      public void RequiresSqlFileExtension() {
        Action act = () => ApplicableMigration.FromStream("V1.0.0__FileWithWrongExtension.txt", new MemoryStream("CREATE SCHEMA [MySchema];"u8.ToArray()));
        act.Should().Throw<ArgumentOutOfRangeException>();
      }

      [Fact]
      public void CreatesApplicableMigration() {
        var migration = ApplicableMigration.FromStream("V1.0.0__ThisIsMyDescription.sql", new MemoryStream("CREATE SCHEMA [MySchema];"u8.ToArray()));

        migration.Should().Be(
            new ApplicableMigration(new SemVersion(1, 0, 0), new Description("ThisIsMyDescription"), new MigrationScriptContent("CREATE SCHEMA [MySchema];"))
        );
      }
    }

    public class EqualsTests : MigrationTests {
      [Fact]
      public void DifferentVersionsAreDifferentMigrations() {
        var migration1 = new ApplicableMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        var migration2 = new ApplicableMigration(new SemVersion(2), new Description("Some description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        migration1.Should().NotBe(migration2);
      }

      [Fact]
      public void DifferentDescriptionsAreDifferentMigrations() {
        var migration1 = new ApplicableMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        var migration2 = new ApplicableMigration(new SemVersion(1), new Description("Another description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        migration1.Should().NotBe(migration2);
      }

      [Fact]
      public void DifferentContentsAreDifferentMigrations() {
        var migration1 = new ApplicableMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        var migration2 = new ApplicableMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("DROP TABLE [MyTable];"));

        migration1.Should().NotBe(migration2);
      }

      [Fact]
      public void EqualVersionDescriptionAndContentsAreEqualMigrations() {
        var migration1 = new ApplicableMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        var migration2 = new ApplicableMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        migration1.Should().Be(migration2);
      }
    }

    public class GetHashCodeTests : ApplicableMigrationTests {
      [Fact]
      public void DifferentVersionsAreDifferentHashCodes() {
        var migration1 = new ApplicableMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        var migration2 = new ApplicableMigration(new SemVersion(2), new Description("Some description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        migration1.GetHashCode().Should().NotBe(migration2.GetHashCode());
      }

      [Fact]
      public void DifferentDescriptionsAreDifferentHashCodes() {
        var migration1 = new ApplicableMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        var migration2 = new ApplicableMigration(new SemVersion(1), new Description("Another description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        migration1.GetHashCode().Should().NotBe(migration2.GetHashCode());
      }

      [Fact]
      public void DifferentContentsAreDifferentHashCodes() {
        var migration1 = new ApplicableMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        var migration2 = new ApplicableMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("DROP TABLE [MyTable];"));

        migration1.GetHashCode().Should().NotBe(migration2.GetHashCode());
      }

      [Fact]
      public void EqualVersionDescriptionAndContentsAreEqualHashCodes() {
        var migration1 = new ApplicableMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        var migration2 = new ApplicableMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        migration1.GetHashCode().Should().Be(migration2.GetHashCode());
      }
    }

    public class ApplyTests : ApplicableMigrationTests {
      [Fact]
      public void RequiresVersionToNotBePrerelease() {
        var migration = new ApplicableMigration(
          new SemVersion(1, 0, 0, "alpha-1"),
          new Description("Some description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        Func<Task> act = async () => await migration.Apply(A.Dummy<IMigrationContext>());

        act.Should().ThrowAsync<PermanentPrereleaseMigrationException>();
      }
    }

    public class AdditionOperatorTests : AppliedMigrationTests {
      readonly ApplicableMigration _applicableMigration;

      public AdditionOperatorTests() {
        _applicableMigration = new ApplicableMigration(
          new SemVersion(2),
          new Description("Some description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));
      }

      [Fact]
      public void RequiredRollbackMigration() {
        Action act = () => {
          var result = _applicableMigration + (RollbackMigration) null;
        };

        act.Should().Throw<ArgumentNullException>();
      }

      [Fact]
      public void ReturnsRollbackEnabledAppliedMigration() {
        var rollbackMigration = new RollbackMigration(new SemVersion(2), new Description("Some description"),
          new MigrationScriptContent("DROP SCHEMA [MySchema];"));

        var result = _applicableMigration + rollbackMigration;

        result.Should().Be(new RollbackEnabledApplicableMigration(
          new SemVersion(2),
          new Description("Some description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"),
          rollbackMigration));
      }
    }
  }
}