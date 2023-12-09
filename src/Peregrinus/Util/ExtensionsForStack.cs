using System;
using System.Collections.Generic;

namespace Peregrinus.Util; 

public static class ExtensionsForStack {
    public static bool TryPop<T>(this Stack<T> stack, out T value) {
        try {
            value = stack.Pop();
            return true;
        }
        catch (InvalidOperationException) {
            value = default(T);
            return false;
        }
    }
}