using System.Collections.Generic;
using System.Linq;
using GoLive.Generator.Caching.Core.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GoLive.Generator.Caching.Core;

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

        retr.HasJsonOptions = classSymbol.GetMembers().OfType<IFieldSymbol>()
            .Any(f => f.Type.ToDisplayString() == "System.Text.Json.JsonSerializerOptions" && f.Name == "memoryCacheJsonSerializerOptions");

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

            if (!attr.Any(e => e.AttributeClass.ToString() == "GoLive.Generator.Caching.CacheAttribute"))
            {
                continue;
            }

            var memberToGenerate = new MemberToGenerate
            {
                Name = methodSymbol.Name,
                Async = methodSymbol.IsAsync
            };

            var returnType = methodSymbol.ReturnType as INamedTypeSymbol;

            if (returnType is INamedTypeSymbol taskType && taskType.OriginalDefinition.ToString() == "System.Threading.Tasks.Task<TResult>")
            {
                returnType = returnType.TypeArguments[0] as INamedTypeSymbol;
                memberToGenerate.returnTypeUnwrappedTask = true;
            }

            memberToGenerate.returnType = returnType;

            var cacheAttr = attr.FirstOrDefault(e => e.AttributeClass.ToString() == "GoLive.Generator.Caching.CacheAttribute");
            memberToGenerate.CacheDuration = (int)cacheAttr.ConstructorArguments.FirstOrDefault(r => r is { Type: { SpecialType: SpecialType.System_Int32 } }).Value;
            memberToGenerate.CacheDurationTimeFrame = (TimeFrame)cacheAttr.ConstructorArguments.FirstOrDefault(r => r is { Kind: TypedConstantKind.Enum }).Value;

            var staleDurationArg = cacheAttr.ConstructorArguments.LastOrDefault(r => r is { Type: { SpecialType: SpecialType.System_Int32 } });
            memberToGenerate.StaleDuration = staleDurationArg.IsNull ? memberToGenerate.CacheDuration * 2 : (int)staleDurationArg.Value;

            var staleDurationTimeFrameArg = cacheAttr.ConstructorArguments.LastOrDefault(r => r is { Kind: TypedConstantKind.Enum });
            memberToGenerate.StaleDurationTimeFrame = staleDurationTimeFrameArg.IsNull ? memberToGenerate.CacheDurationTimeFrame : (TimeFrame)staleDurationTimeFrameArg.Value;
            
            
            if (methodSymbol.IsGenericMethod)
            {
                memberToGenerate.IsGenericMethod = true;
                memberToGenerate.GenericParameters = GetGenericParameters(methodSymbol);
            }

            memberToGenerate.Parameters = methodSymbol.Parameters.Select(f => new ParameterToGenerate { HasDefaultValue = f.HasExplicitDefaultValue, DefaultValue = getDefaultValue(f), Name = f.Name, Type = f.Type.ToDisplayString() }).ToList();
            
            yield return memberToGenerate;
        }
    }
    private static List<GenericParameter> GetGenericParameters(IMethodSymbol methodSymbol)
    {
        var genericParameters = new List<GenericParameter>();

        foreach (var typeParameter in methodSymbol.TypeParameters)
        {
            var genericParameter = new GenericParameter
            {
                Name = typeParameter.Name,
                Constraints = typeParameter.ConstraintTypes.Select(c => c.ToDisplayString()).ToList()
            };

            if (typeParameter.HasConstructorConstraint)
            {
                genericParameter.Constraints.Add("new()");
            }

            if (typeParameter.HasReferenceTypeConstraint)
            {
                genericParameter.Constraints.Add("class");
            }

            if (typeParameter.HasValueTypeConstraint)
            {
                genericParameter.Constraints.Add("struct");
            }

            if (typeParameter.HasNotNullConstraint)
            {
                genericParameter.Constraints.Add("notnull");
            }

            if (typeParameter.HasUnmanagedTypeConstraint)
            {
                genericParameter.Constraints.Add("unmanaged");
            }

            genericParameters.Add(genericParameter);
        }

        return genericParameters;
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