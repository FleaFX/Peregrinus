using System;
using System.IO;
using FakeItEasy;
using FluentAssertions;
using Semver;
using Xunit;

namespace Peregrinus.Model {
  public class RollbackMigrationTests {
    public class FromStreamTests : RollbackMigrationTests {
      [Fact]
      public void RequiresName() {
        Action act = () => ApplicableMigration.FromStream(null, A.Dummy<Stream>());
        act.Should().Throw<ArgumentNullException>();
      }

      [Fact]
      public void RequiresStream() {
        Action act = () => RollbackMigration.FromStream("R1.0.0__Migration.sql", null);
        act.Should().Throw<ArgumentNullException>();
      }

      [Fact]
      public void RequiresCorrectPrefixInFilename() {
        Action act = () => RollbackMigration.FromStream("V1.0.0__NotARollbackMigration.sql", A.Dummy<Stream>());
        act.Should().Throw<ArgumentOutOfRangeException>();
      }

      [Fact]
      public void RequiresValidSemverVersionInFilename() {
        Action act = () => RollbackMigration.FromStream("R1__InvalidSemverVersion.sql", A.Dummy<Stream>());
        act.Should().Throw<ArgumentOutOfRangeException>();
      }

      [Fact]
      public void RequiresDescriptionInFilename() {
        Action act = () => RollbackMigration.FromStream("R1.0.0__.sql", A.Dummy<Stream>());
        act.Should().Throw<ArgumentOutOfRangeException>();
      }

      [Fact]
      public void RequiresSqlFileExtension() {
        Action act = () => RollbackMigration.FromStream("R1.0.0__FileWithWrongExtension.txt", A.Dummy<Stream>());
        act.Should().Throw<ArgumentOutOfRangeException>();
      }

      [Fact]
      public void CreatesRollbackMigration() {
        var migration = RollbackMigration.FromStream("R1.0.0__ThisIsMyDescription.sql", new MemoryStream("DROP SCHEMA [MySchema];"u8.ToArray()));

        migration.Should().Be(new RollbackMigration(new SemVersion(1, 0, 0), new Description("ThisIsMyDescription"), new MigrationScriptContent("DROP SCHEMA [MySchema];")));
      }

    }

    public class AppliesToTests : RollbackMigrationTests {
      readonly RollbackMigration _rollbackMigration;

      public AppliesToTests() {
        _rollbackMigration = new RollbackMigration(new SemVersion(2), new Description("Add schema"), new MigrationScriptContent("DROP SCHEMA [MySchema];"));
      }

      [Fact]
      public void GivenDifferentVersion_ThenReturnsFalse() {
        _rollbackMigration.AppliesTo(new SemVersion(1, 9), new Description("Add schema")).Should().BeFalse();
      }

      [Fact]
      public void GivenDifferentDescription_ThenReturnsFalse() {
        _rollbackMigration.AppliesTo(new SemVersion(2), new Description("Add table")).Should().BeFalse();
      }

      [Fact]
      public void GivenEqualVersionAndEqualDescription_ThenReturnsTrue() {
        _rollbackMigration.AppliesTo(new SemVersion(2), new Description("Add schema")).Should().BeTrue();
      }
    }

    public class EqualsTests : MigrationTests {
      [Fact]
      public void DifferentVersionsAreDifferentMigrations() {
        var migration1 = new RollbackMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("DROP SCHEMA [MySchema];"));

        var migration2 = new RollbackMigration(new SemVersion(2), new Description("Some description"),
          new MigrationScriptContent("DROP SCHEMA [MySchema];"));

        migration1.Should().NotBe(migration2);
      }

      [Fact]
      public void DifferentDescriptionsAreDifferentMigrations() {
        var migration1 = new RollbackMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("DROP SCHEMA [MySchema];"));

        var migration2 = new RollbackMigration(new SemVersion(1), new Description("Another description"),
          new MigrationScriptContent("DROP SCHEMA [MySchema];"));

        migration1.Should().NotBe(migration2);
      }

      [Fact]
      public void DifferentContentsAreDifferentMigrations() {
        var migration1 = new RollbackMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("DROP SCHEMA [MySchema];"));

        var migration2 = new RollbackMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("DROP TABLE [MyTable];"));

        migration1.Should().NotBe(migration2);
      }

      [Fact]
      public void EqualVersionDescriptionAndContentsAreEqualMigrations() {
        var migration1 = new RollbackMigration(new SemVersion(1), new Description("Some description"), new MigrationScriptContent("DROP SCHEMA [MySchema];"));

        var migration2 = new RollbackMigration(new SemVersion(1), new Description("Some description"), new MigrationScriptContent("DROP SCHEMA [MySchema];"));

        migration1.Should().Be(migration2);
      }
    }

    public class GetHashCodeTests : RollbackMigrationTests {
      [Fact]
      public void DifferentVersionsAreDifferentHashCodes() {
        var migration1 = new RollbackMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("DROP SCHEMA [MySchema];"));

        var migration2 = new RollbackMigration(new SemVersion(2), new Description("Some description"),
          new MigrationScriptContent("DROP SCHEMA [MySchema];"));

        migration1.GetHashCode().Should().NotBe(migration2.GetHashCode());
      }

      [Fact]
      public void DifferentDescriptionsAreDifferentHashCodes() {
        var migration1 = new RollbackMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("DROP SCHEMA [MySchema];"));

        var migration2 = new RollbackMigration(new SemVersion(1), new Description("Another description"),
          new MigrationScriptContent("DROP SCHEMA [MySchema];"));

        migration1.GetHashCode().Should().NotBe(migration2.GetHashCode());
      }

      [Fact]
      public void DifferentContentsAreDifferentHashCodes() {
        var migration1 = new RollbackMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("DROP SCHEMA [MySchema];"));

        var migration2 = new RollbackMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("DROP TABLE [MyTable];"));

        migration1.GetHashCode().Should().NotBe(migration2.GetHashCode());
      }

      [Fact]
      public void EqualVersionDescriptionAndContentsAreEqualHashCodes() {
        var migration1 = new RollbackMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("DROP SCHEMA [MySchema];"));

        var migration2 = new RollbackMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("DROP SCHEMA [MySchema];"));

        migration1.GetHashCode().Should().Be(migration2.GetHashCode());
      }
    }
    
  }
}