using CacheTower;
using GoLive.Generator.Caching.CacheTower;
namespace GoLive.Generator.Caching.CacheTower.Playground;


public partial class Class1
{
    protected CacheStack MemoryCache { get; set; }

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

