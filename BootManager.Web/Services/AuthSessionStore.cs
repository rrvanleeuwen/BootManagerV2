using System.Collections.Concurrent;

namespace BootManager.Web.Services;

public interface IAuthSessionStore
{
    string CreateSession();
    bool IsValid(string? sessionId);
    void Remove(string? sessionId);
}

public sealed class AuthSessionStore : IAuthSessionStore
{
    private readonly ConcurrentDictionary<string, byte> _sessions = new();

    public string CreateSession()
    {
        var sessionId = Guid.NewGuid().ToString("N");
        _sessions[sessionId] = 0;
        return sessionId;
    }

    public bool IsValid(string? sessionId)
    {
        return !string.IsNullOrWhiteSpace(sessionId) && _sessions.ContainsKey(sessionId);
    }

    public void Remove(string? sessionId)
    {
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            _sessions.TryRemove(sessionId, out _);
        }
    }
}
