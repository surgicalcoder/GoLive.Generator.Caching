using System.Linq;
using GoLive.Generator.Caching.Core;
using GoLive.Generator.Caching.Core.Model;
using static GoLive.Generator.Caching.Core.SourceCodeGeneratorHelper;

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
            source.AppendLine(2);

            if (!classToGen.HasJsonOptions)
            {
                source.AppendLine("private static JsonSerializerOptions memoryCacheJsonSerializerOptions = new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never };");
            }

            foreach (var member in classToGen.Members.Where(member => member.Async))
            {
                string returnType = member.returnTypeUnwrappedTask ? $"Task<{member.returnType.ToDisplayString()}>" : member.returnType.ToDisplayString();

                if (member.IsGenericMethod)
                {
                    source.AppendLine($"public {(member.Async ? "async" : "")} {returnType} {member.Name.FirstCharToUpper()}" +
                                      $"<{string.Join(",", member.GenericParameters.Select(r=>r.Name))}>" +
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
                    handleAsync(source, classToGen, member);
                }


                if (member.IsGenericMethod)
                {
                    source.AppendLine($"public async Task {member.Name.FirstCharToUpper()}_EvictCache" +
                                      $"<{string.Join(",", member.GenericParameters.Select(r => r.Name))}>" +
                                      $"({string.Join(",", getMethodParameter(member.Parameters))})");
                }
                else
                {
                    source.AppendLine($"public async Task {member.Name.FirstCharToUpper()}_EvictCache" +
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
                    source.AppendLine($"public static string {member.Name.FirstCharToUpper()}_GetCacheKey" +
                                      $"<{string.Join(",", member.GenericParameters.Select(r => r.Name))}>" +
                                      $"({string.Join(",", getMethodParameter(member.Parameters))})");
                }
                else
                {
                    source.AppendLine($"public static string {member.Name.FirstCharToUpper()}_GetCacheKey" +
                                      $"({string.Join(",", getMethodParameter(member.Parameters))})");
                }
                
                source.Append(GetGenericConstraints(member));
                
                handleGetCacheKey(source, classToGen, member);
                
                // Add strongly typed method to set cache
                if (member.IsGenericMethod)
                {
                    source.AppendLine($"public async Task {member.Name.FirstCharToUpper()}_SetCache" +
                                      $"<{string.Join(",", member.GenericParameters.Select(r => r.Name))}>" +
                                      $"({member.returnType} value, TimeSpan duration{(member.Parameters.Count > 0 ? ", " : "")}{string.Join(", ", member.Parameters.Select(p => getParameterWithItemPrefix(p)))})");
                }
                else
                {
                    source.AppendLine($"public async Task {member.Name.FirstCharToUpper()}_SetCache" +
                                      $"({member.returnType} value, TimeSpan duration{(member.Parameters.Count > 0 ? ", " : "")}{string.Join(", ", member.Parameters.Select(p => getParameterWithItemPrefix(p)))})");
                }
                
                source.Append(GetGenericConstraints(member));
                
                handleSetCache(source, classToGen, member);
            }
            
            // Add generic method to get cache key
            source.AppendLine("public static string GetCacheKey(string className, string methodName, params object[] parameters)");
            using (source.CreateBracket())
            {
                source.AppendLine("var serializedParams = parameters != null && parameters.Length > 0 ? JsonSerializer.Serialize(parameters, memoryCacheJsonSerializerOptions) : string.Empty;");
                source.AppendLine("return $\"{className}.{methodName}({serializedParams})\";");
            }

            // Add generic method to set cache
            source.AppendLine("public async Task SetCache<T>(string cacheKey, T value, TimeSpan duration)");
            using (source.CreateBracket())
            {
                source.AppendLine("await memoryCache.SetAsync(cacheKey, value, duration);");
            }
        }
    }

    private static void handleEvictCache(SourceStringBuilder source, ClassToGenerate classToGen, MemberToGenerate member)
    {
        source.AppendLine("await memoryCache.EvictAsync(");
        source.Append("(");
            
        source.AppendLine($"{member.Name.FirstCharToUpper()}_GetCacheKey");
        if (member.IsGenericMethod && member.GenericParameters.Count > 0)
        {
            source.Append($"<{string.Join(",", member.GenericParameters.Select(r => r.Name))}>");
        }
        source.AppendLine("(");
        if (member.Parameters.Count > 0)
        {
            source.Append($"{string.Join(",", member.Parameters.Select(e => e.Name))} ");
        }
        source.AppendLine(")");
        source.AppendLine("));");
    }

    private static void handleAsync(SourceStringBuilder source, ClassToGenerate classToGen, MemberToGenerate member)
    {
        // Get the cache key using our GetCacheKey method
        source.Append("return await memoryCache.GetOrSetAsync");
        source.Append($"<{member.returnType}>");
        source.AppendLine(2);
        using (source.CreateParentheses())
        {
            // First parameter: cache key
            source.Append($"{member.Name.FirstCharToUpper()}_GetCacheKey");
            if (member.IsGenericMethod)
            {
                source.Append($"<{string.Join(",", member.GenericParameters.Select(r => r.Name))}>");
            }
            using (source.CreateParentheses())
            {
                source.Append($"{string.Join(", ", member.Parameters.Select(e => e.Name))}");
            }
            
            // Second parameter: factory method
            source.Append(", async arg => await ");
            source.Append($"{member.Name}");
            if (member.IsGenericMethod)
            {
                source.Append($"<{string.Join(",", member.GenericParameters.Select(r => r.Name))}>");
            }
            using (source.CreateParentheses())
            {
                source.Append($"{string.Join(",", member.Parameters.Select(e => e.Name))}");
            }
            
            // Third parameter: options
            source.Append(", new CacheSettings");
            using (source.CreateParentheses())
            {
                source.Append(GetTimeFrameValue(member.CacheDurationTimeFrame, member.CacheDuration));
                source.Append(", ");
                source.Append(GetTimeFrameValue(member.StaleDurationTimeFrame, member.StaleDuration));
            }
        }
        source.Append(";");
    }

    private static void handleGetCacheKey(SourceStringBuilder source, ClassToGenerate classToGen, MemberToGenerate member)
    {
        using (source.CreateBracket())
        {
            source.Append("return JsonSerializer.Serialize(");
            source.Append($"new Tuple<string {getCommaIfParameters(member.Parameters)} {string.Join(",", member.Parameters.Select(e => e.Type))}> " +
                         $"($\"{classToGen.Namespace}.{classToGen.Name}.{member.Name}{GetGenericParameterTypes(member, "_")}\" ");
            if (member.Parameters.Count > 0)
            {
                source.Append($", {string.Join(",", member.Parameters.Select(e => e.Name))} ");
            }
            source.Append(")");
            source.Append(", memoryCacheJsonSerializerOptions");
            source.AppendLine(");");
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

    private static void handleSetCache(SourceStringBuilder source, ClassToGenerate classToGen, MemberToGenerate member)
    {
        using (source.CreateBracket())
        {
            source.Append("string cacheKey = ");
            source.Append($"{member.Name.FirstCharToUpper()}_GetCacheKey");
            
            if (member.IsGenericMethod)
            {
                source.Append($"<{string.Join(",", member.GenericParameters.Select(r => r.Name))}>");
            }
            
            using (source.CreateParentheses())
            {
                source.Append($"{string.Join(", ", member.Parameters.Select(e => $"Item_{e.Name}"))}");
            }
            source.AppendLine(";" );
            
            source.AppendLine("await memoryCache.SetAsync(cacheKey, value, duration);");
        }
    }

}