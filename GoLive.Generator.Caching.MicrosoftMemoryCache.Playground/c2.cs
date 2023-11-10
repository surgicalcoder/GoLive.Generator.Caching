using Microsoft.Extensions.Caching.Memory;

namespace GoLive.Generator.Caching.MicrosoftMemoryCache.Playground;

public class c2
{
    protected IMemoryCache MemoryCache { get; set; }
    
    private async Task<string> getAsyncValue(string Input1, string wibble = "blarg")
    {
        return "Wibble Wobble";
    }
    
    public async System.Threading.Tasks.Task<string> GetAsyncValue(string Input1, string wibble = "blarg")
    {
        return (await MemoryCache.GetOrCreateAsync(new Tuple<string, string, string>("GoLive.Generator.Caching.MicrosoftMemoryCache.Playground.Class1.getAsyncValue", Input1, wibble), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3);

            return await getAsyncValue(Input1, wibble);
        }))!;
    }
}