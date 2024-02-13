using System;
using System.Linq;
using static GoLive.Generator.Caching.FusionCache.SourceCodeGeneratorHelper;

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
        source.AppendOpenCurlyBracketLine();
        source.AppendLine(2);
        
        foreach (var member in classToGen.Members)
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

            if (member.Async)
            {
                handleAsync(source, classToGen, member);
            }
            else
            {
                handleSync(source, classToGen, member);
            }
            
            source.AppendCloseCurlyBracketLine();
            
            
            
            
            if (member.IsGenericMethod)
            {
                source.AppendLine($"public void {member.Name.FirstCharToUpper()}_EvictCache" +
                                  $"<{string.Join(",", member.GenericConstraints)}>" +
                                  $"({string.Join(",", getMethodParameter(member.Parameters) )})"); 
            }
            else
            {
                source.AppendLine($"public void {member.Name.FirstCharToUpper()}_EvictCache" +
                                  $"({string.Join(",", getMethodParameter(member.Parameters) )})");
            }
            source.AppendOpenCurlyBracketLine();
            source.AppendLine("MemoryCache.Remove(");
            source.Append("(JsonSerializer.Serialize(");
            source.Append($"new Tuple<string {getCommaIfParameters(member.Parameters)} {string.Join(",",member.Parameters.Select(e=>e.Type))}> (\"{classToGen.Namespace}.{classToGen.Name}.{member.Name}\" ");
            if (member.Parameters.Count > 0)
            {
                source.Append($", {string.Join(",", member.Parameters.Select(e=>e.Name))} ");
            }
            source.Append(")");
            source.Append(")");
            source.Append(")");
            source.AppendLine(");");
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
        source.AppendLine(")");
        
        source.Append(", async _ =>");
        source.AppendOpenCurlyBracketLine();
        source.AppendLine($"return await {member.Name}({string.Join(",", member.Parameters.Select(e=>e.Name))}");
        source.AppendLine(");");
        source.AppendCloseCurlyBracketLine();
        source.AppendLine($", {getTimeFrameValue(member)}");
        source.Append(");");
    }

    private static string getTimeFrameValue(MemberToGenerate member) =>
        member.CacheDurationTimeFrame switch
        {
            TimeFrame.Millisecond => $"TimeSpan.FromMilliseconds({member.CacheDuration})",
            TimeFrame.Second => $"TimeSpan.FromSeconds({member.CacheDuration})",
            TimeFrame.Minute => $"TimeSpan.FromMinutes({member.CacheDuration})",
            TimeFrame.Hour => $"TimeSpan.FromHours({member.CacheDuration})",
            TimeFrame.Day => $"TimeSpan.FromDays({member.CacheDuration})",
            _ => throw new ArgumentOutOfRangeException()
        };

    private static void handleSync(SourceStringBuilder source, ClassToGenerate classToGen, MemberToGenerate member)
    {
        source.Append("return MemoryCache.GetOrSet<");
        source.Append(member.returnType.ToDisplayString());
        source.AppendLine(">");
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
        source.AppendOpenCurlyBracketLine();
        source.AppendLine($"return {member.Name}({string.Join(",", member.Parameters.Select(e=>e.Name))}");
        source.AppendLine(");");
        source.AppendCloseCurlyBracketLine();
        source.AppendLine($", {getTimeFrameValue(member)}");
        source.Append(");");
    }    
    
    
    
    private static void handleSync_old(SourceStringBuilder source, ClassToGenerate classToGen, MemberToGenerate member)
    {
        source.Append($"Tuple<string {getCommaIfParameters(member.Parameters)} {string.Join(",",member.Parameters.Select(e=>e.Type))}> cacheKey = " +
                      $"new Tuple<string {getCommaIfParameters(member.Parameters)} {string.Join(",",member.Parameters.Select(e=>e.Type))}>" +
                      $"(\"{classToGen.Namespace}.{classToGen.Name}.{member.Name}\"");

        if (member.Parameters.Count > 0)
        {
            source.Append($", {string.Join(",", member.Parameters.Select(e=>e.Name))} ");
        }
            
        source.AppendLine(");");
        source.AppendLine(2);
            
        source.AppendLine($@"if (MemoryCache.TryGetValue(cacheKey, out {member.returnType} value))
        {{
            return value;
        }}");
            
        source.AppendLine(2);
            
        source.AppendLine($"value = {member.Name}({string.Join(",", member.Parameters.Select(e=>e.Name))});");
        source.AppendLine($"MemoryCache.Set<{member.returnType}>(cacheKey, value);");
        source.AppendLine("return value;");
    }
}