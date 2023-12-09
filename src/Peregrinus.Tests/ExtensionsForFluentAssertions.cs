using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Collections;

namespace Peregrinus {
  public static class ExtensionsForFluentAssertions {
    class ComparisonComparer<T> : IComparer<T> {
      readonly Comparison<T> _comparison;

      public ComparisonComparer(Comparison<T> comparison) {
        _comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
      }

      public int Compare(T x, T y) => _comparison(x, y);
    }

    /// <summary>
    /// Asserts that a collection is in ascending order according to the given <see cref="Comparison{T}"/>.
    /// </summary>
    /// <typeparam name="TAssertions">The type of the collection being asserted.</typeparam>
    /// <param name="assertion">The assertion to build on.</param>
    /// <param name="comparison">The <see cref="Comparison{T}"/> to use when sorting a the collection.</param>
    /// <param name="because">A reason string to use when the assertion fails.</param>
    /// <param name="args">Any additional arguments.</param>
    /// <returns>A <see cref="AndConstraint{T}"/>.</returns>
    public static AndConstraint<SubsequentOrderingAssertions<TAssertions>> BeInAscendingOrder<TAssertions>(
      this GenericCollectionAssertions<TAssertions> assertion, Comparison<TAssertions> comparison, string because = "", params object[] args) {
      return assertion.BeInAscendingOrder(new ComparisonComparer<TAssertions>(comparison), because, args);
    }
  }
}