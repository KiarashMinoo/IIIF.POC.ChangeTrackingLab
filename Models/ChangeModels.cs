namespace IIIF.POC.ChangeTrackingLab.Models;

public sealed record ChangeRow(
    string Path,
    string Kind,
    string? PropertyName,
    string OriginalValue,
    string CurrentValue,
    DateTimeOffset DetectedAtUtc);

public sealed record PersistedChangeEvent(
    Guid ChangeSetId,
    long Revision,
    string ManifestId,
    string Path,
    string Kind,
    string? PropertyName,
    string OriginalJson,
    string CurrentJson,
    DateTimeOffset DetectedAtUtc,
    DateTimeOffset PersistedAtUtc);

public sealed class LabViewModel
{
    public bool HasChanges { get; init; }
    public int PendingChangeCount { get; init; }
    public int FullBytes { get; init; }
    public int ChangedBytes { get; init; }
    public double PayloadReductionPercent { get; init; }
    public long Revision { get; init; }
    public bool HasRemoval { get; init; }
    public string FullJson { get; init; } = "";
    public string ChangedJson { get; init; } = "";
    public IReadOnlyList<ChangeRow> CurrentChanges { get; init; } = [];
    public IReadOnlyList<PersistedChangeEvent> EventLog { get; init; } = [];
}
