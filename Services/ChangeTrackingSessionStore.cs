using IIIF.Manifests.Serializer.Nodes;
using IIIF.POC.ChangeTrackingLab.Models;
using Microsoft.Extensions.Caching.Memory;

namespace IIIF.POC.ChangeTrackingLab.Services;

public sealed class ChangeTrackingSession
{
    public Manifest Manifest { get; } = SampleManifestFactory.Create();
    public List<PersistedChangeEvent> EventLog { get; } = [];
    public long Revision { get; set; }
    public int AddedCanvasCounter { get; set; } = 4;
}

public sealed class ChangeTrackingSessionStore
{
    public static readonly TimeSpan IdleTimeout = TimeSpan.FromMinutes(20);

    private readonly IMemoryCache _cache;
    private readonly object _creationLock = new();

    public ChangeTrackingSessionStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public TResult Read<TResult>(
        string sessionId,
        Func<ChangeTrackingSession, TResult> reader)
    {
        var session = GetOrCreate(sessionId);
        lock (session)
            return reader(session);
    }

    public void Update(
        string sessionId,
        Action<ChangeTrackingSession> update)
    {
        var session = GetOrCreate(sessionId);
        lock (session)
            update(session);
    }

    public void Reset(string sessionId)
    {
        var session = GetOrCreate(sessionId);
        lock (session)
            Set(sessionId, new ChangeTrackingSession());
    }

    private ChangeTrackingSession GetOrCreate(string sessionId)
    {
        if (_cache.TryGetValue(sessionId, out ChangeTrackingSession? session) && session is not null)
            return session;

        lock (_creationLock)
        {
            if (_cache.TryGetValue(sessionId, out session) && session is not null)
                return session;

            session = new ChangeTrackingSession();
            Set(sessionId, session);
            return session;
        }
    }

    private void Set(string sessionId, ChangeTrackingSession session) =>
        _cache.Set(sessionId, session, IdleTimeout);
}
