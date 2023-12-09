using System.Linq;
using Peregrinus.Util;

namespace Peregrinus.Model; 

/// <summary>
/// Represents the checksum of a <see cref="Migration"/>.
/// </summary>
public readonly struct Checksum {
    readonly byte[] _value;

    /// <summary>
    /// An empty <see cref="Checksum"/>.
    /// </summary>
    public static Checksum Empty => new();

    /// <summary>
    /// Initializes a new <see cref="Checksum"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    public Checksum(byte[] value) => _value = value;

    /// <summary>
    /// Casts the given instance to a <see cref="byte[]"/>.
    /// </summary>
    /// <param name="instance">The <see cref="Checksum"/> to cast.</param>
    public static implicit operator byte[](Checksum instance) => instance._value;

    bool Equals(Checksum other) => _value.SequenceEqual(other._value);

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="obj">The object to compare with the current instance. </param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />. </returns>
    public override bool Equals(object obj) => obj is Checksum other && Equals(other);

    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode() =>
        _value is { Length: > 0 } ? _value.Select(v => v.GetHashCode()).Reduce((a, b) => a * 97 ^ b) : 0;
}