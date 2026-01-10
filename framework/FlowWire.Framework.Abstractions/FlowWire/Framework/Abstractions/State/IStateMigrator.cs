namespace FlowWire.Framework.Abstractions.State;

/// <summary>
/// Defines a pipeline for upgrading state schemas from version N to Current.
/// </summary>
public interface IStateMigrator
{
    /// <summary>
    /// Migrates the state object from the stored version to the target version.
    /// </summary>
    /// <param name="state">The deserialized state (potentially an old version DTO).</param>
    /// <param name="fromVersion">The version found in the binary header.</param>
    /// <param name="toVersion">The current version of the [FlowState].</param>
    /// <returns>The migrated state object matching the current schema.</returns>
    object Migrate(object state, int fromVersion, int toVersion);
}
