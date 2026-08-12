# IIIF.POC.ChangeTrackingLab

An ASP.NET Core Razor Pages proof of concept for SDK-native object-graph change tracking in the IIIF Manifest Serializer for .NET.

Core SDK:

https://github.com/KiarashMinoo/IIIF.Manifest.Serializer.Net

## NuGet package

The POC consumes the SDK from NuGet rather than using a project reference:

```xml
<PackageReference Include="IIIF.Manifest.Serializer.Net" Version="3.0.17" />
```

Package source: [nuget.org](https://api.nuget.org/v3/index.json) (no `NuGet.Config` in this repo — the default feed resolves both direct and transitive packages, including `Newtonsoft.Json`).

## What the POC demonstrates

A single Razor Page holds a sample three-canvas Manifest per browser session and exposes buttons that exercise the SDK's change-tracking API:

- `HasChanges`
- `GetChanges()`
- `GetChangeSet()`
- `GetChangedManifest()`
- `SerializeChangedOnly(...)`
- nested paths such as `Items[0].Height`
- scalar property changes
- structural collection additions
- structural collection removals
- original/current values
- full Manifest vs. changed-only payload size
- an append-only simulated revision/event history
- `AcceptChanges()` after a successful commit
- the reason removals belong in a ChangeSet rather than a partial Manifest

## Run

```bash
dotnet restore
dotnet build -c Release
dotnet run
```

The application uses in-memory session state for the demo. Each browser session gets an independent Manifest/change-tracking state, held by `ChangeTrackingSessionStore` and expiring after 20 minutes of inactivity.

## Documentation

Per-folder reference documentation lives under [`docs`](docs/README.md):

- [Models](docs/Models/README.md) — `ChangeRow`, `PersistedChangeEvent`, `LabViewModel`
- [Pages](docs/Pages/README.md) — the `IndexModel` page and its markup
- [Services](docs/Services/README.md) — session storage, Manifest mutations, JSON formatting, the sample Manifest factory
- [wwwroot](docs/wwwroot/README.md) — the demo page's stylesheet

## License

This is a proof-of-concept repository, free to use.
