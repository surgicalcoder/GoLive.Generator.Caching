using System;
using System.Collections.Generic;
using System.Linq;
using GoLive.Generator.Caching.Core;
using GoLive.Generator.Caching.Core.Model;
using Microsoft.CodeAnalysis;
using static GoLive.Generator.Caching.CacheTower.SourceCodeGeneratorHelper;

namespace GoLive.Generator.Caching.CacheTower;

public static class SourceCodeGenerator
{
    public static void Generate(SourceStringBuilder source, ClassToGenerate classToGen)
    {
        source.AppendLine("using System;");
        source.AppendLine("using CacheTower;");
        source.AppendLine("using System.Text.Json;");
        source.AppendLine("using System.Threading.Tasks;");
        source.AppendLine();
        source.AppendLine($"namespace {classToGen.Namespace};");

        source.AppendLine($"public partial class {classToGen.Name}");
        using (source.CreateBracket())
        {
            foreach (var member in classToGen.Members.Where(member => member.Async))
            {
                if (member.IsGenericMethod)
                {
                    source.AppendLine($"public {(member.Async ? "async" : "")} {member.returnType} {member.Name.FirstCharToUpper()}" +
                                      $"<{string.Join(",", member.GenericConstraints)}>" +
                                      $"({string.Join(",", getMethodParameter(member.Parameters))})"); // TODO bool bypassCache = false
                }
                else
                {
                    source.AppendLine($"public {(member.Async ? "async" : "")} {member.returnType} {member.Name.FirstCharToUpper()}" +
                                      $"({string.Join(",", getMethodParameter(member.Parameters))})");
                }

                using (source.CreateBracket())
                {
                    handleAsync(source, classToGen, member);
                }


                if (member.IsGenericMethod)
                {
                    source.AppendLine($"public async Task {member.Name.FirstCharToUpper()}_EvictCache" +
                                      $"<{string.Join(",", member.GenericConstraints)}>" +
                                      $"({string.Join(",", getMethodParameter(member.Parameters))})");
                }
                else
                {
                    source.AppendLine($"public async Task {member.Name.FirstCharToUpper()}_EvictCache" +
                                      $"({string.Join(",", getMethodParameter(member.Parameters))})");
                }

                using (source.CreateBracket())
                {
                    handleEvictCache(source, classToGen, member);
                }
            }
        }
    }

    private static void handleEvictCache(SourceStringBuilder source, ClassToGenerate classToGen, MemberToGenerate member)
    {
        source.AppendLine("await MemoryCache.EvictAsync(");
        source.Append("(JsonSerializer.Serialize(");
        source.Append($"new Tuple<string {getCommaIfParameters(member.Parameters)} {string.Join(",", member.Parameters.Select(e => e.Type))}> (\"{classToGen.Namespace}.{classToGen.Name}.{member.Name}\" ");
        if (member.Parameters.Count > 0)
        {
            source.Append($", {string.Join(",", member.Parameters.Select(e => e.Name))} ");
        }

        source.Append(")");

        if (!member.ObeyIgnoreProperties)
        {
            source.Append(", new JsonSerializerOptions{DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never}");
        }

        source.AppendLine(")));");
    }

    private static void handleAsync(SourceStringBuilder source, ClassToGenerate classToGen, MemberToGenerate member)
    {
        source.Append("return await MemoryCache.GetOrSetAsync");
        source.Append(GenerationHelper.GetTypeParametersForAsyncInvocation(member));
        source.AppendLine(2);
        source.Append("(JsonSerializer.Serialize(");
        source.Append($"new Tuple<string {getCommaIfParameters(member.Parameters)} {string.Join(",", member.Parameters.Select(e=>e.Type))}> (\"{classToGen.Namespace}.{classToGen.Name}.{member.Name}\" ");
        if (member.Parameters.Count > 0)
        {
            source.Append($", {string.Join(",", member.Parameters.Select(e=>e.Name))} ");
        }
        source.Append(")");
        if (!member.ObeyIgnoreProperties)
        {
            source.Append(", new JsonSerializerOptions{DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never}");
        }
        source.AppendLine("), async arg =>");
        source.AppendLine($"await {member.Name}{GenerationHelper.GetTypeParametersForAsyncInvocation(member)}({string.Join(",", member.Parameters.Select(e=>e.Name))}");
        source.Append("), new CacheSettings(");
        source.Append(GenerationHelper.GetTimeFrameValue(member.CacheDurationTimeFrame, member.CacheDuration));
        source.Append(",");
        source.Append(GenerationHelper.GetTimeFrameValue(member.StaleDurationTimeFrame, member.StaleDuration));
        source.Append("));");
    }

}