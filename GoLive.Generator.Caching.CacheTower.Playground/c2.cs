﻿using System.Text.Json;
using CacheTower;

namespace GoLive.Generator.Caching.CacheTower.Playground;

public class c2
{
    protected CacheStack MemoryCache { get; set; }
    
    private async Task<string> getAsyncValue(string Input1, string wibble = "blarg")
    {
        return "Wibble Wobble";
    }
    public async System.Threading.Tasks.Task<string> GetAsyncValue(string Input1, string wibble = "blarg")
    {
        return await MemoryCache.GetOrSetAsync<string>(JsonSerializer.Serialize(new Tuple<string, string, string>("GoLive.Generator.Caching.CacheTower.Playground", Input1, wibble)), async arg =>
            await getAsyncValue(Input1, wibble), new CacheSettings());
    }
    
}