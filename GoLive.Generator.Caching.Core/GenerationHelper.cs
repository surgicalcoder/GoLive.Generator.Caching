using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoLive.Generator.Caching.Core.Model;
using Microsoft.CodeAnalysis;

namespace GoLive.Generator.Caching.Core;

public static  class GenerationHelper
{
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


    public static string GetTypeParametersForAsyncInvocation(MemberToGenerate member)
    {
        if (member.GenericTypeParameters.Any())
        {
            return $"<{string.Join(", ", member.GenericTypeParameters.Select(r => r.Name))}>";
        }

        return string.Empty;
    }

}