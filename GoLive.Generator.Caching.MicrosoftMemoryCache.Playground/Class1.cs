using Microsoft.Extensions.Caching.Memory;

namespace GoLive.Generator.Caching.MicrosoftMemoryCache.Playground;

public partial class Class1
{
    protected IMemoryCache MemoryCache { get; set; }
    
    [Cache(3, TimeFrame.Minute)]
    private string getValue()
    {
        return "Blarg";
    }

    [Cache(3, TimeFrame.Minute)]
    private string getComplicatedValue(string Input1, int Input2 = 3, string wibble = "blarg", DateTime? Value3 = default)
    {
        return $"{Input1} // {Input2}";
    }

    [Cache(3, TimeFrame.Minute)]
    private async Task<string> getAsyncValue(string Input1, string wibble = "blarg")
    {
        return "Wibble Wobble";
    }
}

