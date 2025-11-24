using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

namespace GasStationSystem.Tests.Infrastructure;

public class FakeSession : ISession
{
    private readonly ConcurrentDictionary<string, byte[]> _storage = new();

    public bool IsAvailable => true;
    public string Id { get; } = Guid.NewGuid().ToString();
    public IEnumerable<string> Keys => _storage.Keys;

    public void Clear() => _storage.Clear();

    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void Remove(string key) => _storage.TryRemove(key, out _);

    public void Set(string key, byte[] value) => _storage[key] = value;

    public bool TryGetValue(string key, out byte[] value) =>
        _storage.TryGetValue(key, out value);
}