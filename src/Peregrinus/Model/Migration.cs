using System;
using Semver;

namespace Peregrinus.Model; 

/// <summary>
/// Represents a migration that takes the database up a new version.
/// </summary>
public abstract class Migration {
    protected readonly SemVersion _version;
    protected readonly Description _description;
    protected readonly Checksum _checksum;

    /// <summary>
    /// Initializes a new <see cref="Migration"/>.
    /// </summary>
    /// <param name="version">The version of the migration.</param>
    /// <param name="description">The description of the migration.</param>
    /// <param name="checksum">The checksum of the migration.</param>
    protected Migration(SemVersion version, Description description, Checksum checksum) {
        _version = version;
        _description = description;
        _checksum = checksum;
    }

    /// <summary>
    /// Indicates whether the migration can run within a transaction.
    /// </summary>
    public virtual bool CanRunInTransaction => true;

    /// <summary>Returns a string that represents the current object.</summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"{_version} - {(string) _description}";
    
    /// <summary>
    /// Contains a set of <see cref="Comparison{Migration}">comparisons</see>
    /// </summary>
    public static class Compare {
        /// <summary>
        /// <see cref="Comparison{Migration}"/> that compares two migrations by their <see cref="Version"/>.
        /// </summary>
        public static readonly Comparison<Migration> ByVersion = (left, right) => left._version.CompareTo(right._version);
    }

    bool Equals(Migration other) {
        return Equals(_version, other._version) && _description.Equals(other._description) && _checksum.Equals(other._checksum);
    }

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="obj">The object to compare with the current object. </param>
    /// <returns>
    /// <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (!(obj is Migration)) return false;
        return Equals((Migration) obj);
    }

    /// <summary>Serves as the default hash function. </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() {
        unchecked {
            return ((_version != null ? _version.GetHashCode() : 0) * 397) ^ _description.GetHashCode() ^ _checksum.GetHashCode();
        }
    }
}