using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace Peregrinus.Model {
  public class MigrationScriptContentTests {
    public class EqualsTests : MigrationScriptContentTests {
      [Fact]
      public void EqualValuesAreEqualDescriptions() {
        new MigrationScriptContent("CREATE SCHEMA [MySchema]").Should().Be(new MigrationScriptContent("CREATE SCHEMA [MySchema]"));
      }

      [Fact]
      public void InequalValuesAreInequalDescriptions() {
        new MigrationScriptContent("CREATE SCHEMA [MySchema]").Should().NotBe(new MigrationScriptContent("DROP TABLE [MyTable]"));
      }
    }

    public class GetHashCodeTests : MigrationScriptContentTests {
      [Fact]
      public void EqualValuesHashCodesAreEqualHashCodes() {
        new MigrationScriptContent("CREATE SCHEMA [MySchema]").GetHashCode().Should().Be(new MigrationScriptContent("CREATE SCHEMA [MySchema]").GetHashCode());
      }

      [Fact]
      public void InequalValuesHashCodesAreInequalHashCodes() {
        new MigrationScriptContent("CREATE SCHEMA [MySchema]").GetHashCode().Should().NotBe(new MigrationScriptContent("DROP TABLE [MyTable]").GetHashCode());
      }
    }

    public class FromStreamTests : MigrationScriptContentTests {
      [Fact]
      public void ContainsContentsFromGivenFile() =>
          MigrationScriptContent.FromStream(new MemoryStream("CREATE SCHEMA [MySchema];"u8.ToArray())).Should()
              .Be(new MigrationScriptContent("CREATE SCHEMA [MySchema];"));
    }

    public class ChecksumCastTests : MigrationScriptContentTests {
      [Fact]
      public void ReturnsSha1Checksum() {
        var migrationScriptContent = new MigrationScriptContent("CREATE SCHEMA [MySchema];");
        ((Checksum) migrationScriptContent).Should().Be(new Checksum(Convert.FromBase64String("/sYW2qnA+8nQZ9cbQLFqAo+na5k=")));
      }
    }
  }
}