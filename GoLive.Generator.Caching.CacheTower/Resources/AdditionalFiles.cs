using System;

namespace GoLive.Generator.Caching.CacheTower;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class CacheAttribute : Attribute
{
    public CacheAttribute(int Time, TimeFrame TimeFrame, int StaleTime, TimeFrame StaleTimeFrame, bool ObeyIgnoreProperties = false)
    {
        this.Time = Time;
        this.TimeFrame = TimeFrame;
        this.StaleTime = StaleTime;
        this.StaleTimeFrame = StaleTimeFrame;
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