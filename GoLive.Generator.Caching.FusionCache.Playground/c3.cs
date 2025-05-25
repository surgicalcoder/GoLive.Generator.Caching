using ZiggyCreatures.Caching.Fusion;

namespace GoLive.Generator.Caching.FusionCache.Playground;

public partial class c3
{
    protected IFusionCache memoryCache { get; set; }
    
    [Cache(3, TimeFrame.Minute)]
    private string genericItemTest1<TInput>()
    {
        return "Blarg";
    }
}