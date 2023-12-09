using System;
using System.Threading.Tasks;

namespace Peregrinus {
  public class DeferAssertion<T> {
    T _value;

    DeferAssertion() {
      _value = default(T);
    }

    public void Satisfy(T value) {
      _value = value;
    }

    public static T For(Action<DeferAssertion<T>> act) {
      var assertion = new DeferAssertion<T>();
      act(assertion);
      return assertion._value;
    }

    public static async Task<T> For(Func<DeferAssertion<T>, Task> act) {
      var assertion = new DeferAssertion<T>();
      await act(assertion);
      return assertion._value;
    }
  }
}