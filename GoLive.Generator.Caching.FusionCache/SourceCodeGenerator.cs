using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoLive.Generator.Caching.CacheTower;
using GoLive.Generator.Caching.Core;
using GoLive.Generator.Caching.Core.Model;
using Microsoft.CodeAnalysis;
using static GoLive.Generator.Caching.Core.SourceCodeGeneratorHelper;

namespace GoLive.Generator.Caching.FusionCache;

public static class SourceCodeGenerator
{
    public static void Generate(SourceStringBuilder source, ClassToGenerate classToGen)
    {
        source.AppendLine("using System;");
        source.AppendLine("using ZiggyCreatures.Caching.Fusion;");
        source.AppendLine("using System.Text.Json;");
        source.AppendLine("using System.Threading.Tasks;");
        source.AppendLine();
        
        source.AppendLine($"namespace {classToGen.Namespace};");
        source.AppendLine($"public partial class {classToGen.Name}");

        using (source.CreateBracket())
        {
            source.AppendLine(2);

            if (!classToGen.HasJsonOptions)
            {
                source.AppendLine("private static JsonSerializerOptions memoryCacheJsonSerializerOptions = new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never };");
            }

            foreach (var member in classToGen.Members)
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
                    if (member.Async)
                    {
                        handleAsync(source, classToGen, member);
                    }
                    else
                    {
                        handleSync(source, classToGen, member);
                    }
                }

                if (member.IsGenericMethod)
                {
                    source.AppendLine($"public void {member.Name.FirstCharToUpper()}_EvictCache" +
                                      $"<{string.Join(",", member.GenericConstraints)}>" +
                                      $"({string.Join(",", getMethodParameter(member.Parameters))})");
                }
                else
                {
                    source.AppendLine($"public void {member.Name.FirstCharToUpper()}_EvictCache" +
                                      $"({string.Join(",", getMethodParameter(member.Parameters))})");
                }

                using (source.CreateBracket())
                {
                    source.AppendLine("MemoryCache.Remove(");
                    source.Append("(JsonSerializer.Serialize(");
                    source.Append($"new Tuple<string {getCommaIfParameters(member.Parameters)} {string.Join(",", member.Parameters.Select(e => e.Type))}> (\"{classToGen.Namespace}.{classToGen.Name}.{member.Name}\" ");
                    if (member.Parameters.Count > 0)
                    {
                        source.Append($", {string.Join(",", member.Parameters.Select(e => e.Name))} ");
                    }
                    source.Append(")");

                    source.Append(", memoryCacheJsonSerializerOptions");
                    
                    source.Append(")");
                    source.Append(")");
                    source.AppendLine(");");
                }
            }
        }
    }

    private static void handleAsync(SourceStringBuilder source, ClassToGenerate classToGen, MemberToGenerate member)
    {
        source.Append("return await MemoryCache.GetOrSetAsync");
        source.Append($"<{member.returnType}>");
        source.AppendLine(2);
        
        source.Append("(JsonSerializer.Serialize(");
        source.Append($"new Tuple<string {getCommaIfParameters(member.Parameters)} {string.Join(",",member.Parameters.Select(e=>e.Type))}> (\"{classToGen.Namespace}.{classToGen.Name}.{member.Name}\" ");
        if (member.Parameters.Count > 0)
        {
            source.Append($", {string.Join(",", member.Parameters.Select(e=>e.Name))} ");
        }
        source.Append(")");
        
        source.Append(", memoryCacheJsonSerializerOptions");
        
        source.AppendLine(")");
        
        source.Append(", async _ =>");
        using (source.CreateBracket())
        {
            source.AppendLine($"return await {member.Name}{GetTypeParametersForAsyncInvocation(member)}({string.Join(",", member.Parameters.Select(e => e.Name))}");
            source.AppendLine(");");
        }
        source.Append(GetTimeFrameValue(member.CacheDurationTimeFrame, member.CacheDuration));
        source.Append(");");
    }

    private static void handleSync(SourceStringBuilder source, ClassToGenerate classToGen, MemberToGenerate member)
    {
        source.Append("return MemoryCache.GetOrSet");
        source.Append($"<{member.returnType}>");
        source.AppendLine(2);
        
        source.Append("(JsonSerializer.Serialize(");
        source.Append($"new Tuple<string {getCommaIfParameters(member.Parameters)} {string.Join(",",member.Parameters.Select(e=>e.Type))}> (\"{classToGen.Namespace}.{classToGen.Name}.{member.Name}\" ");
        if (member.Parameters.Count > 0)
        {
            source.Append($", {string.Join(",", member.Parameters.Select(e=>e.Name))} ");
        }
        source.Append(")");
        source.AppendLine(")");
        
        source.Append(", _ =>");
        using (source.CreateBracket())
        {
            source.AppendLine($"return {member.Name}({string.Join(",", member.Parameters.Select(e => e.Name))}");
            source.AppendLine(");");
        }
        source.Append(", ");
        source.Append(GetTimeFrameValue(member.CacheDurationTimeFrame, member.CacheDuration));
        source.Append(");");
    }    
    
}