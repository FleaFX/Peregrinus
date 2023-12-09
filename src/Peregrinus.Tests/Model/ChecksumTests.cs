using FluentAssertions;
using Xunit;

namespace Peregrinus.Model {
  public class ChecksumTests {
    public class EqualsTests : ChecksumTests {
      [Fact]
      public void EqualValuesAreEqualChecksums() {
        new Checksum(new byte[] { 1, 2, 3 }).Should().Be(new Checksum(new byte[] { 1, 2, 3 }));
      }

      [Fact]
      public void DifferentValuesAreDifferentChecksums() {
        new Checksum(new byte[] { 1, 2, 3 }).Should().NotBe(new Checksum(new byte[] { 4, 5, 6 }));
      }
    }

    public class GetHashCodeTests : ChecksumTests {
      [Fact]
      public void EqualValueHashCodesAreEqualChecksumHashCodes() {
        new Checksum(new byte[] { 1, 2, 3 }).GetHashCode().Should().Be(new Checksum(new byte[] { 1, 2, 3 }).GetHashCode());
      }

      [Fact]
      public void DifferentValueHashCodesAreDifferentChecksumHashCodes() {
        new Checksum(new byte[] { 1, 2, 3 }).GetHashCode().Should().NotBe(new Checksum(new byte[] { 4, 5, 6 }).GetHashCode());
      }
    }
  }
}