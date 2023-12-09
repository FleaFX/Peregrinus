using FluentAssertions;
using Xunit;

namespace Peregrinus.Model {
  public class DescriptionTests {
    public class EqualsTests : DescriptionTests {
      [Fact]
      public void EqualValuesAreEqualDescriptions() {
        new Description("ThisIsSomeDescription").Should().Be(new Description("ThisIsSomeDescription"));
      }

      [Fact]
      public void InequalValuesAreInequalDescriptions() {
        new Description("ThisIsSomeDescription").Should().NotBe(new Description("ThisIsSomeOtherDescription"));
      }

      [Fact]
      public void CamelcaseValueEqualsFullSentence() {
        new Description("ThisIsSomeDescription").Should().Be(new Description("This is some description"));
      }

      [Fact]
      public void UnderscoreSeparatedValuesEqualsFullSentence() {
        new Description("This_Is_Some_Description").Should().Be(new Description("This is some description"));
      }
    }

    public class GetHashCodeTests : DescriptionTests {
      [Fact]
      public void EqualValuesAreEqualHashCodes() {
        new Description("ThisIsSomeDescription").GetHashCode().Should().Be(new Description("ThisIsSomeDescription").GetHashCode());
      }

      [Fact]
      public void InequalValuesAreInequalHashCodes() {
        new Description("ThisIsSomeDescription").GetHashCode().Should().NotBe(new Description("ThisIsSomeOtherDescription").GetHashCode());
      }

      [Fact]
      public void CamelcaseValueHashCodeEqualsFullSentenceHashCode() {
        new Description("ThisIsSomeDescription").GetHashCode().Should().Be(new Description("This is some description").GetHashCode());
      }

      [Fact]
      public void UnderscoreSeparatedValueHashCodeEqualsFullSentenceHashCode() {
        new Description("This_Is_Some_Description").GetHashCode().Should().Be(new Description("This is some description").GetHashCode());
      }
    }
  }
}