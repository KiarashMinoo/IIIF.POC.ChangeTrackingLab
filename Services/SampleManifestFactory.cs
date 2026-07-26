using IIIF.Manifests.Serializer.Nodes;
using IIIF.Manifests.Serializer.Properties;

namespace IIIF.POC.ChangeTrackingLab.Services;

public static class SampleManifestFactory
{
    public static Manifest Create()
    {
        var manifest = new Manifest(
            "https://example.org/iiif/change-tracking/manifest",
            new Label("Change Tracking Demo"));

        manifest.AddItem(new Canvas(
            "https://example.org/iiif/change-tracking/canvas/1",
            new Label("Page 1"),
            1200,
            900));

        manifest.AddItem(new Canvas(
            "https://example.org/iiif/change-tracking/canvas/2",
            new Label("Page 2"),
            1200,
            900));

        manifest.AddItem(new Canvas(
            "https://example.org/iiif/change-tracking/canvas/3",
            new Label("Page 3"),
            1200,
            900));

        manifest.ClearChanges();
        return manifest;
    }
}
