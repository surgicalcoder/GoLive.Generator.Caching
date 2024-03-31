using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GoLive.Generator.Caching.CacheTower;

public class Scanner
{
    public static bool CanBeCached(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax { Members.Count: > 0 } c && c.Modifiers.Any(e=> e.Text == "partial" ) && c.Members.Any(r=>r.AttributeLists.Count > 0 );
    }
    
    public static ClassToGenerate ConvertToMapping(INamedTypeSymbol classSymbol)
    {
        ClassToGenerate retr = new();

        retr.Filename = classSymbol.Locations.FirstOrDefault(e => !e.SourceTree.FilePath.EndsWith(".generated.cs")).SourceTree.FilePath;
        retr.Name = classSymbol.Name;
        retr.Namespace = classSymbol.ContainingNamespace.ToDisplayString();
        retr.Members = ConvertToMembers(classSymbol).ToList();

        return retr;
    }
    private static IEnumerable<MemberToGenerate> ConvertToMembers(INamedTypeSymbol classSymbol)
    {
        foreach (var member in classSymbol.GetMembers())
        {
            if (member is not IMethodSymbol
                {
                    DeclaredAccessibility: Accessibility.Private, IsAbstract: false, AssociatedSymbol: null
                } methodSymbol)
            {
                continue;
            }

            var attr = methodSymbol.GetAttributes();

            if (!attr.Any(e => e.AttributeClass.ToString() == "GoLive.Generator.Caching.CacheTower.CacheAttribute"))
            {
                continue;
            }
            
            var memberToGenerate = new MemberToGenerate
            {
                Name = methodSymbol.Name,
                returnType = methodSymbol.ReturnType as INamedTypeSymbol,
                Async = methodSymbol.IsAsync
            };

            var cacheAttr = attr.FirstOrDefault(e => e.AttributeClass.ToString() == "GoLive.Generator.Caching.CacheTower.CacheAttribute");
            memberToGenerate.CacheDuration = (int)cacheAttr.ConstructorArguments.FirstOrDefault(r => r is { Type: { SpecialType: SpecialType.System_Int32 } }).Value;
            memberToGenerate.CacheDurationTimeFrame = (TimeFrame)cacheAttr.ConstructorArguments.FirstOrDefault(r => r is { Kind: TypedConstantKind.Enum }).Value;
            memberToGenerate.StaleDuration = (int)cacheAttr.ConstructorArguments.LastOrDefault(r => r is { Type: { SpecialType: SpecialType.System_Int32 } }).Value;
            memberToGenerate.StaleDurationTimeFrame = (TimeFrame)cacheAttr.ConstructorArguments.LastOrDefault(r => r is { Kind: TypedConstantKind.Enum }).Value;
            
            if (methodSymbol.IsGenericMethod)
            {
                memberToGenerate.IsGenericMethod = true;
                memberToGenerate.GenericConstraints = methodSymbol.TypeParameters.Select(e => e.Name).ToArray(); 
            }

            memberToGenerate.Parameters = methodSymbol.Parameters.Select(f => new ParameterToGenerate { HasDefaultValue = f.HasExplicitDefaultValue, DefaultValue = getDefaultValue(f), Name = f.Name, Type = f.Type.ToDisplayString() }).ToList();
            memberToGenerate.GenericTypeParameters = methodSymbol.TypeParameters.Select(f => new ParameterToGenerate { Name = f.Name }).ToList(); // TODO Change to List<String> at one point
            
            yield return memberToGenerate;
        }
    }

    private static string getDefaultValue(IParameterSymbol f)
    {
        if (!f.HasExplicitDefaultValue)
        {
            return null;
        }
        if (f.Type.SpecialType == SpecialType.System_String)
        {
            return $"\"{f.ExplicitDefaultValue}\"";
        }
        return f.ExplicitDefaultValue?.ToString();
    }
}