using System.Text.Json;
using ZiggyCreatures.Caching.Fusion;

namespace GoLive.Generator.Caching.FusionCache.Playground;

public class c2
{
    protected IFusionCache MemoryCache { get; set; }
    
    public string GetAsyncValue(string Input1, string wibble = "blarg")
    {
        return MemoryCache.GetOrSet<string>(JsonSerializer.Serialize(new Tuple<string, string, string>("GoLive.Generator.Caching.FusionCache.Playground.c1.getAsyncValue", Input1, wibble)), _ =>
        {
            return  retr(Input1, wibble);
        }, TimeSpan.FromMinutes(3));
    }

    private string retr(string Input1, string wibble = "blarg")
    {
        return "hello";
    }
}