namespace GoLive.Generator.Caching.CacheTower;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class CacheAttribute : Attribute
{
    public CacheAttribute(int Time, TimeFrame TimeFrame, int StaleTime, TimeFrame StaleTimeFrame)
    {
        this.Time = Time;
        this.TimeFrame = TimeFrame;
        this.StaleTime = StaleTime;
        this.StaleTimeFrame = StaleTimeFrame;
    }
    
    private int Time { get; set; }
    private TimeFrame TimeFrame { get; set; }
    private int StaleTime { get; set; }
    private TimeFrame StaleTimeFrame { get; set; }
}

public enum TimeFrame
{
    Millisecond,
    Second,
    Minute,
    Hour,
    Day
}