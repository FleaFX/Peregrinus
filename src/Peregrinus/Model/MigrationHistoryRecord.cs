using System;

namespace Peregrinus.Model; 

public class MigrationHistoryRecord {
    /// <summary>
    /// Gets the time of writing the history record, expressed in ticks.
    /// </summary>
    public long TimeStampTicks { get; }

    /// <summary>
    /// Gets the version.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the description.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the checksum.
    /// </summary>
    public byte[] Checksum { get; }

    /// <summary>
    /// Gets the execution time, expressed in ticks.
    /// </summary>
    public long? ExecutionTimeTicks { get; }

    /// <summary>
    /// Parameterless constructor used when materializing from database query results.
    /// </summary>
    public MigrationHistoryRecord() { }

    /// <summary>
    /// Initializes a new <see cref="MigrationHistoryRecord"/>.
    /// </summary>
    /// <param name="version">The version.</param>
    /// <param name="description">The description.</param>
    /// <param name="checksum">The checksum.</param>
    /// <param name="executionTimeTicks">The execution time, expressed in ticks.</param>
    public MigrationHistoryRecord(string version, string description, byte[] checksum, long? executionTimeTicks) {
        TimeStampTicks = DateTimeOffset.Now.Ticks;
        Version = version;
        Description = description;
        Checksum = checksum;
        ExecutionTimeTicks = executionTimeTicks;
    }
}