using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GoLive.Generator.Caching.CacheTower;

[Generator]
public class CachingGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(AddAdditionalFiles);

        var classDeclarations = context.SyntaxProvider.CreateSyntaxProvider(static (s, _) => Scanner.CanBeCached(s),
                static (ctx, _) => GetDeclarations(ctx))
            .Where(static c => c is not null)
            .Select(static (c, _) => Scanner.ConvertToMapping(c));
        
        context.RegisterSourceOutput(classDeclarations.Collect(), static (spc, source) => Execute(spc, source));
    }

    private static void Execute(SourceProductionContext spc, ImmutableArray<ClassToGenerate> source)
    {
        foreach (var toGenerate in source)
        {
            var sourceStringBuilder = new SourceStringBuilder();
            SourceCodeGenerator.Generate(sourceStringBuilder, toGenerate);
            if (sourceStringBuilder.ToString() is { Length: > 0 } s)
            {
                spc.AddSource($"{toGenerate.Name}.g.cs", sourceStringBuilder.ToString());
            }
        }
    }
    
    private void AddAdditionalFiles(IncrementalGeneratorPostInitializationContext context)
    {
        context.AddSource("_additionalfiles.g.cs", ThisAssembly.Resources._Resources.AdditionalFiles.Text);
    }
    
    
    private static INamedTypeSymbol GetDeclarations(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        var symbol = ModelExtensions.GetDeclaredSymbol(context.SemanticModel, classDeclarationSyntax);
        return symbol as INamedTypeSymbol;
    }
}