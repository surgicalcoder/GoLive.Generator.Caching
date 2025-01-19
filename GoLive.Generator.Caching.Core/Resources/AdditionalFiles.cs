using System;

namespace GoLive.Generator.Caching;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class CacheAttribute : Attribute
{
    public CacheAttribute(int Time, TimeFrame TimeFrame, int StaleTime = 0, TimeFrame StaleTimeFrame = default, bool ObeyIgnoreProperties = false)
    {
        this.Time = Time;
        this.TimeFrame = TimeFrame;
        this.StaleTime = StaleTime == 0 ? Time * 2 : StaleTime;
        this.StaleTimeFrame = StaleTimeFrame == default ? TimeFrame : StaleTimeFrame;
        this.ObeyIgnoreProperties = ObeyIgnoreProperties;
    }
    
    private int Time { get; set; }
    private TimeFrame TimeFrame { get; set; }
    private int StaleTime { get; set; }
    private TimeFrame StaleTimeFrame { get; set; }
    private bool ObeyIgnoreProperties { get; set; }
}

public enum TimeFrame
{
    Millisecond,
    Second,
    Minute,
    Hour,
    Day
}