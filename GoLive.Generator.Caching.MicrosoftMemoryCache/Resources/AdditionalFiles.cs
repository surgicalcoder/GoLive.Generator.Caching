namespace GoLive.Generator.Caching.MicrosoftMemoryCache;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class CacheAttribute : Attribute
{
    public CacheAttribute(int Time, TimeFrame TimeFrame)
    {
        this.Time = Time;
        this.TimeFrame = TimeFrame;
    }
    
    private int Time { get; set; }
    private TimeFrame TimeFrame { get; set; }
}

public enum TimeFrame
{
    Millisecond,
    Second,
    Minute,
    Hour,
    Day
}