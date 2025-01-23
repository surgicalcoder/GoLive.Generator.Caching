using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GoLive.Generator.Caching.Core.Model;
using Microsoft.CodeAnalysis;
using Resourcer;

namespace GoLive.Generator.Caching.Core;

public static class SourceCodeGeneratorHelper
{
    public static string GetEmbeddedResourceContents()
    {
        return Resource.AsString("GoLive.Generator.Caching.Core.Resources.AdditionalFiles.cs");
    }

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

    public static bool IsSimpleType(ITypeSymbol type)
    {
        return type.SpecialType != SpecialType.None || type.Name == "String";
    }

    public static IEnumerable<string> GetParameterTypes(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol { IsGenericType: true } namedType)
        {
            foreach (var paramType in namedType.TypeArguments.SelectMany(GetParameterTypes))
            {
                yield return paramType;
            }
        }
        else
        {
            /*if (!IsSimpleType(type))
            {*/
            yield return type.Name;
            // }
        }
    }

    public static string GetTimeFrameValue(TimeFrame TimeFrame, int Value) =>
        TimeFrame switch
        {
            TimeFrame.Millisecond => $"TimeSpan.FromMilliseconds({Value})",
            TimeFrame.Second => $"TimeSpan.FromSeconds({Value})",
            TimeFrame.Minute => $"TimeSpan.FromMinutes({Value})",
            TimeFrame.Hour => $"TimeSpan.FromHours({Value})",
            TimeFrame.Day => $"TimeSpan.FromDays({Value})",
            _ => throw new ArgumentOutOfRangeException()
        };

    public static string GetGenericConstraints(MemberToGenerate member)
    {
        if (member?.GenericParameters == null || member.GenericParameters.Count == 0)
        {
            return string.Empty;
        }

        if (member.GenericParameters.SelectMany(r=>r.Constraints).Any())
        {
            return $"where {string.Join(" and ", member.GenericParameters.Select(r => $"{r.Name} : {string.Join(",", r.Constraints)}"))}";
        }

        return string.Empty;
    }

    public static string GetGenericParameterTypes(MemberToGenerate member, string separator)
    {
        if (member?.GenericParameters == null || member.GenericParameters.Count == 0)
        {
            return string.Empty;
        }

        return $"{separator}{string.Join(separator, member.GenericParameters.Select(r => $"{{typeof({r.Name})}}"))}";
    }
}