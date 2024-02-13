# GoLive.Generator.Caching

Autogenerate wrappers for caching, by using parameters and full namespaces.

It turns:
```csharp
[Cache(3, TimeFrame.Minute, 5, TimeFrame.Minute)]
private async Task<string> getAsyncValue(string Input1, string wibble = "blarg")
{
    return "Wibble Wobble";
}
```

to:

```csharp
[Cache(3, TimeFrame.Minute, 5, TimeFrame.Minute)]
private async Task<string> getAsyncValue(string Input1, string wibble = "blarg")
{
    return "Wibble Wobble";
}

public async System.Threading.Tasks.Task<string> GetAsyncValue(string Input1, string wibble = "blarg")
{
    return await MemoryCache.GetOrSetAsync<string>(JsonSerializer.Serialize(new Tuple<string, string, string>("GoLive.Generator.Caching.CacheTower.Playground.Class1.getAsyncValue", Input1, wibble)), async arg => await getAsyncValue(Input1, wibble), new CacheSettings(TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(5)));
}

```

Comes in two...no, three flavours:
- CacheTower
- Microsoft's IMemoryCache
- FusionCache