# IIIF.POC.ChangeTrackingLab

A lightweight **ASP.NET Core Razor Pages** proof of concept demonstrating SDK-native object-graph change tracking in the IIIF Manifest Serializer for .NET.

Core SDK:

https://github.com/KiarashMinoo/IIIF.Manifest.Serializer.Net

## NuGet package

The POC consumes the SDK from NuGet rather than using a project reference:

```xml
<PackageReference Include="IIIF.Manifest.Serializer.Net" Version="3.0.13" />
```

## What the POC demonstrates

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
dotnet run
```

The application uses in-memory session state for the demo. Each browser session gets an independent Manifest/change-tracking state.

## Recommended repository name

```text
IIIF.POC.ChangeTrackingLab
```

## Recommended GitHub About description

**Proof-of-concept ASP.NET Core Razor Pages app demonstrating IIIF object-graph change tracking, changed-only manifests, delta event history, and payload comparison.**

## Shorter repository description

**Razor Pages POC for IIIF Manifest change tracking, ChangeSets, changed-only serialization, and delta history.**

## More technical repository description

**ASP.NET Core Razor Pages proof of concept for IIIF Manifest Serializer change tracking, including nested graph deltas, ChangeSets, changed-only output, and append-only revision history.**

## Suggested repository topics

```text
iiif
dotnet
csharp
aspnet-core
razor-pages
change-tracking
event-driven
digital-libraries
proof-of-concept
```
