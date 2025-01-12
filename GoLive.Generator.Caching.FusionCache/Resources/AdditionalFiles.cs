using System;

namespace GoLive.Generator.Caching.FusionCache;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class CacheAttribute : Attribute
{
    public CacheAttribute(int Time, TimeFrame TimeFrame, bool ObeyIgnoreProperties = false)
    {
        this.Time = Time;
        this.TimeFrame = TimeFrame;
        this.ObeyIgnoreProperties = ObeyIgnoreProperties;
    }
    
    private int Time { get; set; }
    private TimeFrame TimeFrame { get; set; }
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