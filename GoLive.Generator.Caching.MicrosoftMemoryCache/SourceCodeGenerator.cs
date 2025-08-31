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
                
                // Add strongly typed method to get cache key
                if (member.IsGenericMethod)
                {
                    source.AppendLine($"public static Tuple<string {getCommaIfParameters(member.Parameters)} {string.Join(",", member.Parameters.Select(e => e.Type))}> {member.Name.FirstCharToUpper()}_GetCacheKey" +
                                      $"<{string.Join(",", member.GenericParameters.Select(r => r.Name))}>" +
                                      $"({string.Join(",", getMethodParameter(member.Parameters))})");
                }
                else
                {
                    source.AppendLine($"public static Tuple<string {getCommaIfParameters(member.Parameters)} {string.Join(",", member.Parameters.Select(e => e.Type))}> {member.Name.FirstCharToUpper()}_GetCacheKey" +
                                      $"({string.Join(",", getMethodParameter(member.Parameters))})");
                }
                
                source.Append(GetGenericConstraints(member));
                
                handleGetCacheKey(source, classToGen, member);
                
                // Add strongly typed method to set cache
                if (member.IsGenericMethod)
                {
                    source.AppendLine($"public void {member.Name.FirstCharToUpper()}_SetCache" +
                                      $"<{string.Join(",", member.GenericParameters.Select(r => r.Name))}>" +
                                      $"({member.returnType} value, TimeSpan duration{(member.Parameters.Count > 0 ? ", " : "")}{string.Join(", ", member.Parameters.Select(p => getParameterWithItemPrefix(p)))})");
                }
                else
                {
                    source.AppendLine($"public void {member.Name.FirstCharToUpper()}_SetCache" +
                                      $"({member.returnType} value, TimeSpan duration{(member.Parameters.Count > 0 ? ", " : "")}{string.Join(", ", member.Parameters.Select(p => getParameterWithItemPrefix(p)))})");
                }
                
                source.Append(GetGenericConstraints(member));
                
                handleSetCache(source, classToGen, member);
            }

            // Add generic method to set cache
            source.AppendLine("public void SetCache<T>(object cacheKey, T value, TimeSpan duration)");
            using (source.CreateBracket())
            {
                source.AppendLine("var options = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = duration };");
                source.AppendLine("memoryCache.Set(cacheKey, value, options);");
            }
        }
    }

    private static void handleEvictCache(SourceStringBuilder source, ClassToGenerate classToGen, MemberToGenerate member)
    {
        using (source.CreateParentheses("memoryCache.Remove"))
        {
            source.Append($"{member.Name.FirstCharToUpper()}_GetCacheKey");
            
            if (member.IsGenericMethod)
            {
                source.Append($"<{string.Join(",", member.GenericParameters.Select(r => r.Name))}>");
            }
            
            using (source.CreateParentheses())
            {
                source.Append($"{string.Join(", ", member.Parameters.Select(e => $"{e.Name}"))}");
            }
            
        }
        source.Append(";");
    }

    private static void handleAsync(SourceStringBuilder source, ClassToGenerate classToGen, MemberToGenerate member)
    {
        source.Append("return await memoryCache.GetOrCreateAsync(");
        source.Append($"{member.Name.FirstCharToUpper()}_GetCacheKey");
            
        if (member.IsGenericMethod)
        {
            source.Append($"<{string.Join(",", member.GenericParameters.Select(r => r.Name))}>");
        }
            
        using (source.CreateParentheses())
        {
            source.Append($"{string.Join(", ", member.Parameters.Select(e => $"{e.Name}"))}");
        }
        
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
                source.Append($"<{string.Join(",", member.GenericParameters.Select(r => r.Name))}>");
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
        source.Append($"var cacheKey = " );

        source.Append($"{member.Name.FirstCharToUpper()}_GetCacheKey");
            
        if (member.IsGenericMethod)
        {
            source.Append($"<{string.Join(",", member.GenericParameters.Select(r => r.Name))}>");
        }
            
        using (source.CreateParentheses())
        {
            source.Append($"{string.Join(", ", member.Parameters.Select(e => $"{e.Name}"))}");
        }
        source.Append(";");
        source.AppendLine(2);
            
        source.AppendLine($@"if (memoryCache.TryGetValue(cacheKey, out {member.returnType} value))
        {{
            return value;
        }}");
            
        source.AppendLine(2);
            
        source.AppendLine($"value = {member.Name}");
        if (member.IsGenericMethod)
        {
            source.Append($"<{string.Join(",", member.GenericParameters.Select(r => r.Name))}>");
        }
        
        source.Append($"({string.Join(",", member.Parameters.Select(e=>e.Name))});");
        source.AppendLine($"memoryCache.Set<{member.returnType}>(cacheKey, value);");
        source.AppendLine("return value;");
    }

    private static void handleGetCacheKey(SourceStringBuilder source, ClassToGenerate classToGen, MemberToGenerate member)
    {
        using (source.CreateBracket())
        {
            source.Append("return new Tuple<string");
            if (member.Parameters.Count > 0)
            {
                source.Append($", {string.Join(",", member.Parameters.Select(e => e.Type))}");
            }
            source.Append(">(");
            source.Append($"\"{classToGen.Namespace}.{classToGen.Name}.{member.Name}{GetGenericParameterTypes(member, "_")}\"");
            if (member.Parameters.Count > 0)
            {
                source.Append($", {string.Join(",", member.Parameters.Select(e => e.Name))}");
            }
            source.AppendLine(");");
        }
    }

    private static void handleSetCache(SourceStringBuilder source, ClassToGenerate classToGen, MemberToGenerate member)
    {
        using (source.CreateBracket())
        {
            source.Append("var cacheKey = ");
            source.Append($"{member.Name.FirstCharToUpper()}_GetCacheKey");
            
            if (member.IsGenericMethod)
            {
                source.Append($"<{string.Join(",", member.GenericParameters.Select(r => r.Name))}>");
            }
            
            using (source.CreateParentheses())
            {
                source.Append($"{string.Join(", ", member.Parameters.Select(e => $"Item_{e.Name}"))}");
            }
            source.AppendLine(";");
            
            source.AppendLine("var options = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = duration };");
            source.AppendLine("memoryCache.Set(cacheKey, value, options);");
        }
    }

    private static string getParameterWithItemPrefix(ParameterToGenerate param)
    {
        string baseParameter = $"{param.Type} Item_{param.Name}";
        if (param.HasDefaultValue)
        {
            // Handle default values more carefully
            if (string.IsNullOrEmpty(param.DefaultValue) || param.DefaultValue == "default" || param.DefaultValue == "null")
            {
                baseParameter += " = default";
            }
            else
            {
                baseParameter += $" = {param.DefaultValue}";
            }
        }
        return baseParameter;
    }
}