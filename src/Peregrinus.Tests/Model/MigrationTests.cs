using System;
using FluentAssertions;
using Semver;
using Xunit;

namespace Peregrinus.Model {
  public class MigrationTests {

    public class EqualsTests : MigrationTests {
      [Fact]
      public void DifferentAppliedAndApplicableVersionsAreDifferentMigrations() {
        var migration1 = new ApplicableMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        var migration2 = new AppliedMigration(new SemVersion(2), new Description("Some description"), new Checksum(Convert.FromBase64String("/sYW2qnA+8nQZ9cbQLFqAo+na5k=")));

        migration1.Should().NotBe(migration2);
      }

      [Fact]
      public void DifferentAppliedAndApplicableDescriptionsAreDifferentMigrations() {
        var migration1 = new ApplicableMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        var migration2 = new AppliedMigration(new SemVersion(1), new Description("Another description"), new Checksum(Convert.FromBase64String("/sYW2qnA+8nQZ9cbQLFqAo+na5k=")));

        migration1.Should().NotBe(migration2);
      }

      [Fact]
      public void DifferentAppliedAndApplicableContentsAndChecksumsAreDifferentMigrations() {
        var migration1 = new ApplicableMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        var migration2 = new AppliedMigration(new SemVersion(1), new Description("Some description"), new Checksum(new byte[] { 1, 2, 3, 4, 5 }));

        migration1.Should().NotBe(migration2);
      }

      [Fact]
      public void EqualAppliedAndApplicableVersionDescriptionAndContentChecksumsAreEqualMigrations() {
        var migration1 = new ApplicableMigration(new SemVersion(1), new Description("Some description"), new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        var migration2 = new AppliedMigration(new SemVersion(1), new Description("Some description"), new Checksum(Convert.FromBase64String("/sYW2qnA+8nQZ9cbQLFqAo+na5k=")));

        migration1.Should().Be(migration2);
      }
    }

    public class GetHashCodeTests : MigrationTests {
      [Fact]
      public void DifferentAppliedAndApplicableVersionsAreDifferentHashCodes() {
        var migration1 = new ApplicableMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        var migration2 = new AppliedMigration(new SemVersion(2), new Description("Some description"), new Checksum(Convert.FromBase64String("/sYW2qnA+8nQZ9cbQLFqAo+na5k=")));

        migration1.GetHashCode().Should().NotBe(migration2.GetHashCode());
      }

      [Fact]
      public void DifferentAppliedAndApplicableDescriptionsAreDifferentHashCodes() {
        var migration1 = new ApplicableMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        var migration2 = new AppliedMigration(new SemVersion(1), new Description("Another description"), new Checksum(Convert.FromBase64String("/sYW2qnA+8nQZ9cbQLFqAo+na5k=")));

        migration1.GetHashCode().Should().NotBe(migration2.GetHashCode());
      }

      [Fact]
      public void DifferentAppliedAndApplicableContentsAndChecksumsAreDifferentHashCodes() {
        var migration1 = new ApplicableMigration(new SemVersion(1), new Description("Some description"),
          new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        var migration2 = new AppliedMigration(new SemVersion(1), new Description("Some description"), new Checksum(new byte[] { 1, 2, 3, 4, 5 }));

        migration1.GetHashCode().Should().NotBe(migration2.GetHashCode());
      }

      [Fact]
      public void EqualAppliedAndApplicableVersionDescriptionAndContentChecksumsAreEqualHashCodes() {
        var migration1 = new ApplicableMigration(new SemVersion(1), new Description("Some description"), new MigrationScriptContent("CREATE SCHEMA [MySchema];"));

        var migration2 = new AppliedMigration(new SemVersion(1), new Description("Some description"), new Checksum(Convert.FromBase64String("/sYW2qnA+8nQZ9cbQLFqAo+na5k=")));

        migration1.GetHashCode().Should().Be(migration2.GetHashCode());
      }
    }
  }
}