using System;
using FluentAssertions;
using Semver;
using Xunit;

namespace Peregrinus.Model {
  public class AppliedMigrationTests {
    public class EqualsTests : AppliedMigrationTests {
      [Fact]
      public void DifferentVersionsAreDifferentMigrations() {
        var migration1 = new AppliedMigration(new SemVersion(1), new Description("Some description"), new Checksum(new byte[] { 1, 2, 3, 4, 5 }));

        var migration2 = new AppliedMigration(new SemVersion(2), new Description("Some description"), new Checksum(new byte[] { 1, 2, 3, 4, 5 }));

        migration1.Should().NotBe(migration2);
      }

      [Fact]
      public void DifferentDescriptionsAreDifferentMigrations() {
        var migration1 = new AppliedMigration(new SemVersion(1), new Description("Some description"), new Checksum(new byte[] { 1, 2, 3, 4, 5 }));

        var migration2 = new AppliedMigration(new SemVersion(1), new Description("Another description"), new Checksum(new byte[] { 1, 2, 3, 4, 5 }));

        migration1.Should().NotBe(migration2);
      }

      [Fact]
      public void DifferentChecksumsAreDifferentMigrations() {
        var migration1 = new AppliedMigration(new SemVersion(1), new Description("Some description"), new Checksum(new byte[] { 1, 2, 3, 4, 5 }));

        var migration2 = new AppliedMigration(new SemVersion(1), new Description("Some description"), new Checksum(new byte[] { 5, 4, 3, 2, 1 }));

        migration1.Should().NotBe(migration2);
      }

      [Fact]
      public void EqualVersionDescriptionAndChecksumsAreEqualMigrations() {
        var migration1 = new AppliedMigration(new SemVersion(1), new Description("Some description"), new Checksum(new byte[] { 1, 2, 3, 4, 5 }));

        var migration2 = new AppliedMigration(new SemVersion(1), new Description("Some description"), new Checksum(new Byte[] { 1, 2, 3, 4, 5 }));

        migration1.Should().Be(migration2);
      }
    }

    public class GetHashCodeTests : MigrationTests {
      [Fact]
      public void DifferentVersionsAreDifferentHashCodes() {
        var migration1 = new AppliedMigration(new SemVersion(1), new Description("Some description"), new Checksum(new byte[] { 1, 2, 3, 4, 5 }));

        var migration2 = new AppliedMigration(new SemVersion(2), new Description("Some description"), new Checksum(new byte[] { 1, 2, 3, 4, 5 }));

        migration1.GetHashCode().Should().NotBe(migration2.GetHashCode());
      }

      [Fact]
      public void DifferentDescriptionsAreDifferentHashCodes() {
        var migration1 = new AppliedMigration(new SemVersion(1), new Description("Some description"), new Checksum(new byte[] { 1, 2, 3, 4, 5 }));

        var migration2 = new AppliedMigration(new SemVersion(1), new Description("Another description"), new Checksum(new byte[] { 1, 2, 3, 4, 5 }));

        migration1.GetHashCode().Should().NotBe(migration2.GetHashCode());
      }

      [Fact]
      public void DifferentChecksumsAreDifferentHashCodes() {
        var migration1 = new AppliedMigration(new SemVersion(1), new Description("Some description"), new Checksum(new byte[] { 1, 2, 3, 4, 5 }));

        var migration2 = new AppliedMigration(new SemVersion(1), new Description("Some description"), new Checksum(new byte[] { 5, 4, 3, 2, 1 }));

        migration1.GetHashCode().Should().NotBe(migration2.GetHashCode());
      }

      [Fact]
      public void EqualVersionDescriptionAndChecksumsAreEqualHashCodes() {
        var migration1 = new AppliedMigration(new SemVersion(1), new Description("Some description"), new Checksum(new byte[] { 1, 2, 3, 4, 5 }));

        var migration2 = new AppliedMigration(new SemVersion(1), new Description("Some description"), new Checksum(new byte[] { 1, 2, 3, 4, 5 }));

        migration1.GetHashCode().Should().Be(migration2.GetHashCode());
      }
    }

    public class AdditionOperatorTests : AppliedMigrationTests {
      readonly AppliedMigration _appliedMigration;

      public AdditionOperatorTests() {
        _appliedMigration = new AppliedMigration(
          new SemVersion(2),
          new Description("Some description"),
          new Checksum(new byte[] { 1, 2, 3, 4}));
      }

      [Fact]
      public void RequiredRollbackMigration() {
        Action act = () => {
          var result = _appliedMigration + (RollbackMigration) null;
        };

        act.Should().Throw<ArgumentNullException>();
      }

      [Fact]
      public void ReturnsRollbackEnabledAppliedMigration() {
        var rollbackMigration = new RollbackMigration(new SemVersion(2), new Description("Some description"),
          new MigrationScriptContent("DROP SCHEMA [MySchema];"));

        var result = _appliedMigration + rollbackMigration;

        result.Should().Be(new RollbackEnabledAppliedMigration(
          new SemVersion(2),
          new Description("Some description"),
          new Checksum(new byte[] {1, 2, 3, 4}),
          rollbackMigration));
      }
    }
  }
}