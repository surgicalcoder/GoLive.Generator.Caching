using System.Text.Json;
using CacheTower;
using GoLive.Generator.Caching;
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

    public async System.Threading.Tasks.Task<string> GetAsyncValue3(string Input1, string wibble = "blarg")
    {
        return await MemoryCache.GetOrSetAsync<string>(JsonSerializer.Serialize(new Tuple<string, string, string>("GoLive.Generator.Caching.CacheTower.Playground.Class1.getAsyncValue", Input1, wibble), new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never }), async arg => await getAsyncValue(Input1, wibble), new CacheSettings(TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(5)));
    }


    [Cache(3, TimeFrame.Minute, 5, TimeFrame.Minute, ObeyIgnoreProperties: true)]
    private async Task<string> dontIgnorePropertiesTest(string SingleInput)
    {
        return "blarg";
    }
}

