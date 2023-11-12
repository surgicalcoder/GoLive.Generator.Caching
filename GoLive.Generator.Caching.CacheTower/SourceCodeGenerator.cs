using System;
using System.Linq;
using static GoLive.Generator.Caching.CacheTower.SourceCodeGeneratorHelper;

namespace GoLive.Generator.Caching.CacheTower;

public static class SourceCodeGenerator
{
    public static void Generate(SourceStringBuilder source, ClassToGenerate classToGen)
    {
        source.AppendLine("using CacheTower;");
        source.AppendLine("using System.Text.Json;");
        source.AppendLine($"namespace {classToGen.Namespace};");

        source.AppendLine($"public partial class {classToGen.Name}");
        source.AppendOpenCurlyBracketLine();
        source.AppendLine(2);
        
        foreach (var member in classToGen.Members.Where(member => member.Async))
        {
            if (member.IsGenericMethod)
            {
                source.AppendLine($"public {(member.Async ? "async" : "")} {member.returnType} {member.Name.FirstCharToUpper()}" +
                                  $"<{string.Join(",", member.GenericConstraints)}>" +
                                  $"({string.Join(",", getMethodParameter(member.Parameters) )})"); // TODO bool bypassCache = false
            }
            else
            {
                source.AppendLine($"public {(member.Async ? "async" : "")} {member.returnType} {member.Name.FirstCharToUpper()}" +
                                  $"({string.Join(",", getMethodParameter(member.Parameters) )})");
            }
            
            source.AppendOpenCurlyBracketLine();

            handleAsync(source, classToGen, member);
            
            source.AppendCloseCurlyBracketLine();
            
            if (member.IsGenericMethod)
            {
                source.AppendLine($"public async Task {member.Name.FirstCharToUpper()}_EvictCache" +
                                  $"<{string.Join(",", member.GenericConstraints)}>" +
                                  $"({string.Join(",", getMethodParameter(member.Parameters) )})"); 
            }
            else
            {
                source.AppendLine($"public async Task {member.Name.FirstCharToUpper()}_EvictCache" +
                                  $"({string.Join(",", getMethodParameter(member.Parameters) )})");
            }
            
            source.AppendOpenCurlyBracketLine();
            //await MemoryCache.EvictAsync(JsonSerializer.Serialize(new Tuple<string, string, string>("GoLive.Generator.Caching.CacheTower.Playground", Input1, wibble)));
            source.AppendLine("await MemoryCache.EvictAsync(");
            source.Append("(JsonSerializer.Serialize(");
            source.Append($"new Tuple<string {getCommaIfParameters(member.Parameters)} {string.Join(",",member.Parameters.Select(e=>e.Type))}> (\"{classToGen.Namespace}.{classToGen.Name}.{member.Name}\" ");
            if (member.Parameters.Count > 0)
            {
                source.Append($", {string.Join(",", member.Parameters.Select(e=>e.Name))} ");
            }
            source.Append(")");
            source.AppendLine(")));");
            source.AppendCloseCurlyBracketLine();
        }
        
        source.AppendCloseCurlyBracketLine();
    }

    private static void handleAsync(SourceStringBuilder source, ClassToGenerate classToGen, MemberToGenerate member)
    {
        source.Append("return await MemoryCache.GetOrSetAsync<");
        source.Append(member.returnType.TypeArguments.FirstOrDefault().ToDisplayString());
        source.AppendLine(">");
        source.AppendLine(2);
        source.Append("(JsonSerializer.Serialize(");
        source.Append($"new Tuple<string {getCommaIfParameters(member.Parameters)} {string.Join(",",member.Parameters.Select(e=>e.Type))}> (\"{classToGen.Namespace}.{classToGen.Name}.{member.Name}\" ");
        if (member.Parameters.Count > 0)
        {
            source.Append($", {string.Join(",", member.Parameters.Select(e=>e.Name))} ");
        }
        source.Append(")");
        source.AppendLine("), async arg =>");
        source.AppendLine($"await {member.Name}({string.Join(",", member.Parameters.Select(e=>e.Name))}");
        source.Append("), new CacheSettings(");
        source.Append(getCacheTimeFrameValue(member));
        source.Append(",");
        source.Append(getStaleTimeFrameValue(member));
        source.Append("));");
    }

    private static string getCacheTimeFrameValue(MemberToGenerate member) =>
        member.CacheDurationTimeFrame switch
        {
            TimeFrame.Millisecond => $"TimeSpan.FromMilliseconds({member.CacheDuration})",
            TimeFrame.Second => $"TimeSpan.FromSeconds({member.CacheDuration})",
            TimeFrame.Minute => $"TimeSpan.FromMinutes({member.CacheDuration})",
            TimeFrame.Hour => $"TimeSpan.FromHours({member.CacheDuration})",
            TimeFrame.Day => $"TimeSpan.FromDays({member.CacheDuration})",
            _ => throw new ArgumentOutOfRangeException()
        };    
    
    private static string getStaleTimeFrameValue(MemberToGenerate member) =>
        member.StaleDurationTimeFrame switch
        {
            TimeFrame.Millisecond => $"TimeSpan.FromMilliseconds({member.StaleDuration})",
            TimeFrame.Second => $"TimeSpan.FromSeconds({member.StaleDuration})",
            TimeFrame.Minute => $"TimeSpan.FromMinutes({member.StaleDuration})",
            TimeFrame.Hour => $"TimeSpan.FromHours({member.StaleDuration})",
            TimeFrame.Day => $"TimeSpan.FromDays({member.StaleDuration})",
            _ => throw new ArgumentOutOfRangeException()
        };
}