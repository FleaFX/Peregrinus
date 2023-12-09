using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Peregrinus.Util; 

public static class Diagnostics {
    /// <summary>
    /// Performs the given <see cref="Func{T}"/> while recording how long it took to do it.
    /// </summary>
    /// <param name="act">The <see cref="Func{T}"/> to perform.</param>
    /// <returns>A <see cref="TimeSpan"/> that indicates the time it took to perform the given <see cref="Func{T}"/>.</returns>
    public static (TimeSpan, T) TimeOperation<T>(Func<T> act) {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = act();
        stopwatch.Stop();
        return (stopwatch.Elapsed, result);
    }

    /// <summary>
    /// Performs the given <see cref="Func{T}"/> while recording how long it took to do it.
    /// </summary>
    /// <param name="act">The <see cref="Func{T}"/> to perform.</param>
    /// <returns>A <see cref="TimeSpan"/> that indicates the time it took to perform the given <see cref="Func{T}"/>.</returns>
    public static async Task<(TimeSpan, T)> TimeOperationAsync<T>(Func<Task<T>> act) {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await act();
        stopwatch.Stop();
        return (stopwatch.Elapsed, result);
    }
}