using System.Text.Json;
using CacheTower;
using GoLive.Generator.Caching;
namespace GoLive.Generator.Caching.CacheTower.Playground;


public partial class Class1
{
    protected CacheStack memoryCache { get; set; }


    [Cache(3, TimeFrame.Minute, 5, TimeFrame.Minute)]
    private async Task<T> getWithGenericConstraint<T>(string Input1, string wibble = "blarg") where T : new()
    {
        return new T();
    }

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



    /*public async Task<string> GetAsyncValue(string Input1, string wibble = "blarg")
    {
        return await memoryCache.GetOrSetAsync<string>(JsonSerializer.Serialize(new Tuple<string, string, string>($"GoLive.Generator.Caching.CacheTower.Playground.Class1.getAsyncValue", Input1, wibble), new JsonSerializerOptions()), async arg => await getAsyncValue<string>(Input1, wibble), new CacheSettings(TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(5)));
    }*/

}

