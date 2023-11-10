using System.Collections;
using System.Collections.Generic;

namespace GoLive.Generator.Caching.MicrosoftMemoryCache;

public static class SourceCodeGeneratorHelper
{
    public static string getCommaIfParameters(IList member)
    {
        return (member.Count > 1 ? "," : "" );
    }

    public static IEnumerable<string> getMethodParameter(List<ParameterToGenerate> memberParameters)
    {
        foreach (var param in memberParameters)
        {
            var retr = $"{param.Type} {param.Name}";

            if (param.HasDefaultValue)
            {
                retr = param.DefaultValue == null ? $"{retr} = default" : $"{retr} = {param.DefaultValue}";
            }

            yield return retr;
        }
    }
}