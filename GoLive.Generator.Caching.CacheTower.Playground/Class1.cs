﻿using System.Text.Json;
using CacheTower;
using GoLive.Generator.Caching;
namespace GoLive.Generator.Caching.CacheTower.Playground;


public partial class Class1
{
    protected CacheStack MemoryCache { get; set; }
    protected static JsonSerializerOptions MemoryCache_JsonOptions = new() { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never };

    [Cache(3, TimeFrame.Minute, 5, TimeFrame.Minute)]
    private async Task<string> getAsyncValue(string Input1, string wibble = "blarg")
    {
        return "Wibble Wobble";
    }
    
    [Cache(3, TimeFrame.Minute, 5, TimeFrame.Minute)]
    private async Task<string> getAsync2(string SingleInput)
    {
        return "blarg";
    }

    [Cache(3, TimeFrame.Minute, 5, TimeFrame.Minute, ObeyIgnoreProperties: true)]
    private async Task<string> dontIgnorePropertiesTest(string SingleInput)
    {
        return "blarg";
    }
}

