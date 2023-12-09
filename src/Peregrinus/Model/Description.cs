using System.Text.RegularExpressions;

namespace Peregrinus.Model; 

/// <summary>
/// Represents the description of a <see cref="Migration"/>.
/// </summary>
public struct Description {
    readonly string _value;

    static readonly Regex CamelcaseFormat = new Regex(@"([a-z])_{0,}([A-Z])");

    /// <summary>
    /// Represents the lack of a description. Not allowed to be used for normal migrations.
    /// </summary>
    public static Description None = new Description();

    /// <summary>
    /// Initializes a new <see cref="Description"/>.
    /// </summary>
    /// <param name="value">The value of the description.</param>
    public Description(string value) {
        _value = CamelcaseFormat.Replace(value, m => $"{m.Groups[1].Value} {m.Groups[2].Value.ToLower()}");
    }

    /// <summary>
    /// Casts the given instance to a <see cref="string"/>.
    /// </summary>
    /// <param name="instance">The <see cref="Description"/> to cast.</param>
    public static implicit operator string(Description instance) => instance._value;
}