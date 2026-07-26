using System.Text;
using IIIF.Manifests.Serializer;
using IIIF.Manifests.Serializer.ChangeTracking;
using IIIF.Manifests.Serializer.Nodes;
using IIIF.Manifests.Serializer.Properties;
using IIIF.POC.ChangeTrackingLab.Models;

namespace IIIF.POC.ChangeTrackingLab.Services;

public static class ChangeTrackingLabService
{
    public static LabViewModel BuildView(ChangeTrackingSession state)
    {
        var changes = state.Manifest.GetChanges();

        var rows = changes
            .Select(change => new ChangeRow(
                change.Path,
                change.Kind.ToString(),
                change.PropertyName,
                ChangeValueFormatter.ToDisplayValue(change.OriginalValue),
                ChangeValueFormatter.ToDisplayValue(change.CurrentValue),
                change.ChangedAtUtc))
            .ToList();

        var fullJson = IiifSerializer.Serialize(state.Manifest);
        var changedJson = IiifSerializer.SerializeChangedOnly(state.Manifest);

        var fullBytes = Encoding.UTF8.GetByteCount(fullJson);
        var changedBytes = Encoding.UTF8.GetByteCount(changedJson);

        return new LabViewModel
        {
            HasChanges = state.Manifest.HasChanges,
            PendingChangeCount = rows.Count,
            FullBytes = fullBytes,
            ChangedBytes = changedBytes,
            PayloadReductionPercent = fullBytes == 0
                ? 0
                : Math.Max(0, (1d - ((double)changedBytes / fullBytes)) * 100d),
            Revision = state.Revision,
            HasRemoval = changes.Any(x => x.Kind == IiifChangeKind.CollectionItemRemoved),
            FullJson = fullJson,
            ChangedJson = changedJson,
            CurrentChanges = rows,
            EventLog = state.EventLog.ToList()
        };
    }

    public static void ChangeRights(ChangeTrackingSession state)
    {
        var rights = Rights.CcBy;
        state.Manifest.SetRights(rights);
    }

    public static void ChangeFirstCanvasHeight(ChangeTrackingSession state)
    {
        var canvas = state.Manifest.Items.OfType<Canvas>().FirstOrDefault();
        if (canvas is not null)
            canvas.SetHeight((canvas.Height ?? 0) + 100);
    }

    public static void RenameSecondCanvas(ChangeTrackingSession state)
    {
        var canvas = state.Manifest.Items.OfType<Canvas>().Skip(1).FirstOrDefault();
        if (canvas is not null)
            canvas.SetLabel([new Label($"Page 2 — edited {DateTimeOffset.UtcNow:HH:mm:ss}")]);
    }

    public static void AddCanvas(ChangeTrackingSession state)
    {
        var number = state.AddedCanvasCounter++;

        state.Manifest.AddItem(new Canvas(
            $"https://example.org/iiif/change-tracking/canvas/{number}",
            new Label($"Added Page {number}"),
            1200,
            900));
    }

    public static void RemoveLastCanvas(ChangeTrackingSession state)
    {
        var canvas = state.Manifest.Items.OfType<Canvas>().LastOrDefault();
        if (canvas is not null)
            state.Manifest.RemoveItem(canvas);
    }

    public static void CommitDelta(ChangeTrackingSession state)
    {
        var changeSet = state.Manifest.GetChangeSet();

        if (changeSet.Changes.Count == 0)
            return;

        var changeSetId = Guid.NewGuid();
        var persistedAt = DateTimeOffset.UtcNow;
        var revision = ++state.Revision;

        foreach (var change in changeSet.Changes)
        {
            state.EventLog.Insert(0, new PersistedChangeEvent(
                changeSetId,
                revision,
                changeSet.RootId,
                change.Path,
                change.Kind.ToString(),
                change.PropertyName,
                ChangeValueFormatter.ToStableJson(change.OriginalValue),
                ChangeValueFormatter.ToStableJson(change.CurrentValue),
                change.ChangedAtUtc,
                persistedAt));
        }

        state.Manifest.AcceptChanges();
    }
}
