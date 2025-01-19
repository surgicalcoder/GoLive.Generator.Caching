namespace GoLive.Generator.Caching.Core.Model;

public class ParameterToGenerate
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string DefaultValue { get; set; }
    public bool HasDefaultValue { get; set; }
}