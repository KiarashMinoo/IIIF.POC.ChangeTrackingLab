# IIIF.POC.ChangeTrackingLab
A lightweight ASP.NET Core Razor Pages proof of concept illustrating object-graph change tracking within the [IIIF Manifest Serializer for .NET](https://github.com/KiarashMinoo/IIIF.Manifest.Serializer.Net).
The project is about asking and answering this single question:
> can I track changes to a part of a IIIF Manifest as opposed to the whole, and subsequently act upon them?
The POC makes the SDK's change tracking capabilities accessible to experimentation by letting you:

- mutate a sample Manifest
- check what exactly has changed
- see the difference in full vs changed-only serialization
- commit the changes as a delta to a simulated history
---
## What It Does
This sample application tracks:
- `HasChanges`
- `GetChanges()`
- `GetChangeSet()`
- `GetChangedManifest()`
- `IiifSerializer.SerializeChangedOnly(...)`
- nested object-graph changes (e.g. `Items[0].Height`)
- original and current values for properties
- additions
- deletions
- full vs changed-only payload sizes
- simulated append-only revision history
- `AcceptChanges()` as a new baseline
It also demonstrates the difference between a changed-only Manifest and a ChangeSet, where the latter preserves facts that cannot be expressed as a partial IIIF Manifest, e.g. a Canvas having previously existed but since been deleted.
Removals will still be present in:
```c#
manifest.GetChangeSet().Changes
```
rather than:
```c#
manifest.GetChangedManifest()
```
---
## How It Works

The POC generally follows this lifecycle:
```
Create / load Manifest
↓
Clean tracking baseline
↓
Update Manifest or nested Canvas
↓
HasChanges
↓
GetChanges / GetChangeSet
↓
Inspect full vs. changed-only output
↓
Commit simulated delta
↓
AcceptChanges
↓
New baseline
```
---
## Example
```c#
var manifest = new Manifest("https://example.org/manifest", new Label("Example"));
var canvas = new Canvas("http://example.org/canvas/p1", new Label("Page 1"), 1200, 900);
manifest.AddItem(canvas);
manifest.ClearChanges();
canvas.SetHeight(1300);
Console.WriteLine(manifest.HasChanges);
// True
var changes = manifest.GetChanges();
foreach (var change in changes) {
  Console.WriteLine($"{change.Path}: {change.OriginalValue} -> {change.CurrentValue}");
}
```
A nested property change will report a path such as:
```
Items[0].Height
```
with:
```
Kind:     Modified
OriginalValue: 1200
CurrentValue: 1300
```
The change will be detected even though the Canvas was updated via reference.
---
## SDK Uses Pull-Based Tracking
The SDK uses a pull-based algorithm to track changes, meaning that calling `HasChanges()` or `GetChanges()` traverses the object graph, rather than individual objects notifying their parent when changed.
This means that code such as this:
```c#
manifest.AddItem(canvas);
manifest.ClearChanges();
canvas.SetHeight(1500);
bool changed = manifest.HasChanges;
```
will still correctly report `true` for `changed`.
There is no explicit lifetime / attachment tracking necessary for changes to be picked up, however this comes at the cost of traversal overhead when changes are requested. This could be significant for extremely large graphs, or extremely frequent polling.
---
## Changed-Only Serialization
Serialization is normally done to a complete, live Manifest:
```c#
var fullJson = IiifSerializer.Serialize(manifest);
```
To serialize just the changes, we need to explicitly ask:
```c#
var changedJson = IiifSerializer.SerializeChangedOnly(manifest);
```
The POC displays both for comparison, including a UTF-8 byte size comparison. This demonstrates how a smaller delta payload may be preferable when most of a larger Manifest remains the same.
No claims are made about performance or compression ratio; the application simply demonstrates the ratio for the example Manifest and changes.
---
## ChangeSet vs Changed Manifest
To obtain a complete change record:
```c#
var changeSet = manifest.GetChangeSet();
```
A ChangeSet contains:
```
RootId
RootType
CreatedAtUtc
Changes
ChangedManifest
```
### Changes
The complete list of changes, including deletions.
### ChangedManifest
A best-effort partial IIIF Manifest containing current data that can be represented in a valid IIIF Manifest document.
For applications that wish to persist an audit trail, process events, or synchronize with external systems, the `Changes` list should be consulted first.
---
## Simulated Revision History
The POC contains a small in-memory event timeline.
When "Commit delta" is selected:
1. `GetChangeSet()` is called
2. A local revision number is generated for the event
3. Original and current values are converted to JSON snapshots
4. The JSON event and change records are placed in memory
5. `AcceptChanges()` sets the new baseline
The revision number and event history are specific to this sample application; they are not exposed by the SDK.
This is intentional.
The SDK knows what has changed. It is up to the consuming application to decide what to do with that information: persist, version, publish, or synchronize.
---
## No Database
There is no database implementation in this repository.
There is no:
- EF Core
- PostgreSQL
- TimescaleDB
- event store
- message broker
The event history is kept in server memory only, to avoid bloat and keep the scope of this POC to the change tracking API itself.
Persistence and time-series considerations are discussed later in this document.
---
## Sessions
Each browser session gets its own change tracking context. This is done using:
- ASP.NET Core Session for browser session identification
- `IMemoryCache` for server-side lab state
- 20 minute idle timeout
- synchronized access to session state for simplicity
This lets you experiment in multiple browser tabs, or have concurrent users testing the application, without a database.
As mentioned, this is a lightweight POC and the state can be lost on server restart or cache eviction.
---
## Tech
- .NET 10
- ASP.NET Core
- Razor Pages
- C#
- IIIF Manifest Serializer for .NET
- NuGet
---
## NuGet
The SDK is referenced via NuGet rather than directly from this repository:
```xml
```
Or:
```bash
dotnet add package IIIF.Manifest.Serializer.Net
```
NuGet:
https://www.nuget.org/packages/IIIF.Manifest.Serializer.Net
---
## Try It Yourself
Clone the repository:
```bash
git clone https://github.com/KiarashMinoo/IIIF.POC.ChangeTrackingLab.git
cd IIIF.POC.ChangeTrackingLab
```
Restore dependencies:
```bash
dotnet restore
```
Run:
```bash
dotnet run
```
Browse to the ASP.NET Core URL reported in the console.
---
## Some Things To Do
When you run the application:
1. Change the Manifest rights
2. Change the first Canvas height
3. Rename the second Canvas
4. Add a Canvas
5. Remove a Canvas
6. Inspect the pending ChangeSet
7. See the difference between full and changed-only JSON
8. Compare the payload sizes
9. Commit the delta
10. Make another change
Deleting a Canvas is an interesting exercise, as it demonstrates why a ChangeSet is necessary for certain operations.
---
## Where This Might Be Useful
With the change tracking features described in this document, you might consider the following use cases for your application:
### Audit history
Rather than storing opaque snapshots, store the changes alongside the resource. This provides a rich history for users or admins to inspect.
### Selective cache invalidation
Use change paths to determine what downstream caches need to be invalidated when a resource changes.
```
Items[4].Label
```
could be a pattern your application recognizes and acts upon.
### Search indexing
Only reindex a resource if a property relevant to search has changed.
### Events
Project change tracking information to your event bus or message queue system of choice:
- Kafka
- RabbitMQ
- NATS
- Azure Service Bus
- AWS SNS/SQS
- etc
### Webhooks
Rather than sending a complete Manifest on every update, send a compact change tracking representation your webhook consumers can understand.
### Synchronization
If there is a baseline resource your systems can agree upon, change tracking can be used to synchronize resources with minimal data transfer.
### Time-series
Project changes to your time-series database, if you have one:
- PostgreSQL
- TimescaleDB
- InfluxDB
- etc
This can be useful if you want to analyze:
- changes per hour
- most changed properties
- Canvas additions / removals
- editorial activity by Manifest
- metadata churn over time
- etc
These are general considerations; this repository is not concerned with implementing them.
---
## Change Tracking Is Not JSON Patch
A change path such as:
```
Items[0].Height
```
is an SDK object model path.
It is not:
- JSON Pointer
- JSONPath
- RFC 6902 JSON Patch
`IiifChangeEntry` is a domain-specific change representation.
You may want to write an adapter to another format if you need to.
---
## IIIF Change Discovery
The SDK's internal object tracking and the IIIF Change Discovery API are orthogonal concerns.
While object tracking can identify:
```
Items[0].Height: 1200 -> 1300
```
the IIIF Change Discovery API can communicate that:
> this Manifest was updated
A consuming system could internally use a ChangeSet while publishing a Change Discovery activity for external harvesters to consume.
IIIF Change Discovery:
https://iiif.io/api/discovery/1.0/
---
## Scope
The POC has a deliberately limited scope.
It is not:
- a full IIIF Manifest editor
- a IIIF viewer
- a database persistence example
- an event-sourcing framework
- a distributed synchronization protocol
- a JSON Patch implementation
It exists simply to make the SDK's change tracking capabilities easy to inspect and experiment with.
---
## Similar Projects
### IIIF Manifest Serializer for .NET
The main library this project depends on can be found here:
https://github.com/KiarashMinoo/IIIF.Manifest.Serializer.Net
It contains the version-aware IIIF Presentation API models and serialization code for .NET.
The serializer can be used to read and write manifests, serialize them to JSON with change tracking, etc.
### IIIF Version Lab
This POC's counterpart focused on IIIF version detection:
https://github.com/KiarashMinoo/IIIF.POC.VersionLab
It demonstrates detecting and converting between IIIF Presentation API versions.
---
## Resources
- Core SDK: https://github.com/KiarashMinoo/IIIF.Manifest.Serializer.Net
- NuGet: https://www.nuget.org/packages/IIIF.Manifest.Serializer.Net
- Change tracking: https://github.com/KiarashMinoo/IIIF.Manifest.Serializer.Net/blob/main/docs/CHANGE_TRACKING.md
- IIIF: https://iiif.io/
- IIIF Presentation API: https://iiif.io/api/presentation/3.0/
- IIIF Change Discovery: https://iiif.io/api/discovery/1.0/
---
## License
This proof of concept is licensed according to the licenses defined in this repository.
The IIIF Manifest Serializer for .NET is available under the MIT License.
