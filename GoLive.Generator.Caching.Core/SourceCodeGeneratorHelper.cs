using System.Collections;
using System.Collections.Generic;
using GoLive.Generator.Caching.Core;
using GoLive.Generator.Caching.Core.Model;

namespace GoLive.Generator.Caching.CacheTower;

public static class SourceCodeGeneratorHelper
{
    public static string getCommaIfParameters(IList member)
    {
        return (member.Count > 0 ? "," : "" );
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