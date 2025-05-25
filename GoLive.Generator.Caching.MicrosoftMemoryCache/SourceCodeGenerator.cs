using System.Linq;
using GoLive.Generator.Caching.CacheTower;
using GoLive.Generator.Caching.Core;
using GoLive.Generator.Caching.Core.Model;
using static GoLive.Generator.Caching.Core.SourceCodeGeneratorHelper;

namespace GoLive.Generator.Caching.MicrosoftMemoryCache;

public static class SourceCodeGenerator
{
    public static void Generate(SourceStringBuilder source, ClassToGenerate classToGen)
    {
        source.AppendLine("using System;");
        source.AppendLine("using Microsoft.Extensions.Caching.Memory;");
        source.AppendLine("using System.Threading.Tasks;");
        source.AppendLine();
        
        source.AppendLine($"namespace {classToGen.Namespace};");
        source.AppendLine($"public partial class {classToGen.Name}");
        using (source.CreateBracket())
        {
            source.AppendLine(2);

            foreach (var member in classToGen.Members)
            {
                string returnType = member.returnTypeUnwrappedTask ? $"Task<{member.returnType.ToDisplayString()}>" : member.returnType.ToDisplayString();

                if (member.IsGenericMethod)
                {
                    source.AppendLine($"public {(member.Async ? "async" : "")} {returnType} {member.Name.FirstCharToUpper()}" +
                                      $"<{string.Join(",", member.GenericParameters.Select(r => r.Name))}>" +
                                      $"({string.Join(",", getMethodParameter(member.Parameters))})"); // TODO bool bypassCache = false
                }
                else
                {
                    source.AppendLine($"public {(member.Async ? "async" : "")} {returnType} {member.Name.FirstCharToUpper()}" +
                                      $"({string.Join(",", getMethodParameter(member.Parameters))})");
                }

                source.Append(GetGenericConstraints(member));

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
                                      $"<{string.Join(",", member.GenericParameters.Select(r => r.Name))}>" +
                                      $"({string.Join(",", getMethodParameter(member.Parameters))})");
                }
                else
                {
                    source.AppendLine($"public void {member.Name.FirstCharToUpper()}_EvictCache" +
                                      $"({string.Join(",", getMethodParameter(member.Parameters))})");
                }

                source.Append(GetGenericConstraints(member));

                using (source.CreateBracket())
                {
                    handleEvictCache(source, classToGen, member);
                }

            }
        }
    }

    private static void handleEvictCache(SourceStringBuilder source, ClassToGenerate classToGen, MemberToGenerate member)
    {
        using (source.CreateParentheses("memoryCache.Remove"))
        {
            source.Append($"new Tuple<string {getCommaIfParameters(member.Parameters)} {string.Join(",", member.Parameters.Select(e => e.Type))}> " +
                          $"($\"{classToGen.Namespace}.{classToGen.Name}.{member.Name}{GetGenericParameterTypes(member, "_")}\" ");
            if (member.Parameters.Count > 0)
            {
                source.Append($", {string.Join(",", member.Parameters.Select(e => e.Name))} ");
            }

            source.Append(")");
        }
        source.Append(";");
    }

    private static void handleAsync(SourceStringBuilder source, ClassToGenerate classToGen, MemberToGenerate member)
    {
        source.Append("return await memoryCache.GetOrCreateAsync(");
        source.Append($"new Tuple<string {getCommaIfParameters(member.Parameters)} {string.Join(",", member.Parameters.Select(e => e.Type))}> (\"{classToGen.Namespace}.{classToGen.Name}.{member.Name}\" ");

        if (member.Parameters.Count > 0)
        {
            source.Append($", {string.Join(",", member.Parameters.Select(e => e.Name))} ");
        }

        source.AppendLine(")");

        source.Append(", async entry =>");
        using (source.CreateBracket())
        {
            source.Append("entry.AbsoluteExpirationRelativeToNow = ");
            source.Append(GetTimeFrameValue(member.CacheDurationTimeFrame, member.CacheDuration));
            source.AppendLine(";");
            source.AppendLine($"return ");

            source.Append($"await {member.Name}");

            if (member.IsGenericMethod)
            {
                source.Append($"<{member.returnType}>");
            }

            using (source.CreateParentheses())
            {
                source.Append($"{string.Join(",", member.Parameters.Select(e => e.Name))}");
            }
            source.Append(";");
        }

        source.Append(");");
    }

    private static void handleSync(SourceStringBuilder source, ClassToGenerate classToGen, MemberToGenerate member)
    {
        source.Append($"Tuple<string {getCommaIfParameters(member.Parameters)} {string.Join(",", member.Parameters.Select(e => e.Type))}> cacheKey = " +
                      $"new Tuple<string {getCommaIfParameters(member.Parameters)} {string.Join(",", member.Parameters.Select(e => e.Type))}>" +
                      $"(\"{classToGen.Namespace}.{classToGen.Name}.{member.Name}\"");

        if (member.Parameters.Count > 0)
        {
            source.Append($", {string.Join(",", member.Parameters.Select(e=>e.Name))} ");
        }
            
        source.AppendLine(");");
        source.AppendLine(2);
            
        source.AppendLine($@"if (memoryCache.TryGetValue(cacheKey, out {member.returnType} value))
        {{
            return value;
        }}");
            
        source.AppendLine(2);
            
        source.AppendLine($"value = {member.Name}({string.Join(",", member.Parameters.Select(e=>e.Name))});");
        source.AppendLine($"memoryCache.Set<{member.returnType}>(cacheKey, value);");
        source.AppendLine("return value;");
    }
}