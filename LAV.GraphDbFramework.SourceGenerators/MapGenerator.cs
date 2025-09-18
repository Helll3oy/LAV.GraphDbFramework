using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
//using Minerals.StringCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LAV.GraphDbFramework.SourceGenerators;

[Generator]
public class GraphMapGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (s, _) => IsSyntaxTargetForGeneration(s),
                transform: (ctx, _) => GetSemanticTargetForGeneration(ctx)
            )
            .Where(c => c is not null);

        var compilationAndClasses = context.CompilationProvider.Combine(classProvider.Collect());

        context.RegisterSourceOutput(compilationAndClasses, (spc, source) =>
        {
            foreach (var classSyntax in source.Right)
            {
                Execute(spc, source.Left, classSyntax);
            }
        });
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var model = context.SemanticModel;

        foreach (var attributeList in classDeclaration.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var symbol = model.GetSymbolInfo(attribute).Symbol;
                if (symbol != null && symbol.ToDisplayString().StartsWith("LAV.GraphDbFramework.Core.Attributes.GraphMapAttribute"))
                    return classDeclaration;
            }
        }

        return null;
    }

    private static void Execute(SourceProductionContext context, Compilation compilation, ClassDeclarationSyntax? classSyntax)
    {
        if (classSyntax is null) return;

        var semanticModel = compilation.GetSemanticModel(classSyntax.SyntaxTree);
        var typeSymbol = semanticModel.GetDeclaredSymbol(classSyntax) as INamedTypeSymbol;

        if (typeSymbol is null) return;

        // Далее ваш оригинальный код
        var namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();
        var className = typeSymbol.Name;
        var properties = typeSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic)
            .ToList();

        var source = GenerateMappingClass(namespaceName, className, properties);
        context.AddSource($"{className}Mapper.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static string GenerateMappingClass(string namespaceName, string className, List<IPropertySymbol> properties)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Collections.Frozen;");
        sb.AppendLine("using LAV.GraphDbFramework.Core;");
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName}");
        sb.AppendLine("{");

		sb.AppendLine($"    public static partial class {className}Mapper");
        sb.AppendLine("    {");

		sb.AppendLine($"        public static {className} MapFromNode(IGraphDbRecord record, string nodeAlias)");
		sb.AppendLine("        {");
		sb.AppendLine($"           var properties = record.GetNode(nodeAlias).Properties;");
		sb.AppendLine($"           var obj = new {className}();");

		foreach (var property in properties)
		{
			var propertyName = property.Name;
			var mappedPropertyName = propertyName.ToSnakeCase();
			var propertyType = property.Type.ToDisplayString();

            sb.AppendLine($"           if (properties.TryGetValue(\"{mappedPropertyName}\", out var {mappedPropertyName}))");
			sb.AppendLine($"           {{");
            sb.AppendLine($"               obj.{propertyName} = ({propertyType}){mappedPropertyName};");
			sb.AppendLine($"           }}");

			//sb.AppendLine($"            if (record.TryGet<{propertyType}>(\"{mappedPropertyName}\", out var {mappedPropertyName}))");
			//sb.AppendLine($"            {{");
			//if (propertyType.Equals("System.DateTime"))
			//{
			//	sb.AppendLine($"                obj.{propertyName} = {mappedPropertyName}.Kind == DateTimeKind.Utc ? {mappedPropertyName}.ToLocalTime() : {mappedPropertyName};");
			//}
			//if (propertyType.Equals("System.DateTime?"))
			//{
			//	sb.AppendLine($"                obj.{propertyName} = {mappedPropertyName}.Value.Kind == DateTimeKind.Utc ? {mappedPropertyName}.Value.ToLocalTime() : {mappedPropertyName};");
			//}
			//else
			//{
			//	sb.AppendLine($"                obj.{propertyName} = {mappedPropertyName};");
			//}
			//sb.AppendLine($"            }}");
		}

		sb.AppendLine("            return obj;");
		sb.AppendLine("        }");
		sb.AppendLine();


		// Метод маппинга
		sb.AppendLine($"        public static {className} MapFromRecord(IGraphDbRecord record)");
        sb.AppendLine("        {");
		//sb.AppendLine($"           var properties = record.GetNode(nodeAlias).Properties;");
		sb.AppendLine($"           var obj = new {className}();");
                                                                                                             
		foreach (var property in properties)
		{
			var propertyName = property.Name;
			var mappedPropertyName = propertyName.ToSnakeCase();
			var propertyType = property.Type.ToDisplayString();

			sb.AppendLine($"            if (record.TryGet<{propertyType}>(\"{mappedPropertyName}\", out var {mappedPropertyName}))");
			sb.AppendLine($"            {{");
            if (propertyType.Equals("System.DateTime"))
            {
				sb.AppendLine($"                obj.{propertyName} = {mappedPropertyName}.Kind == DateTimeKind.Utc ? {mappedPropertyName}.ToLocalTime() : {mappedPropertyName};");
            }
			if (propertyType.Equals("System.DateTime?"))
			{
				sb.AppendLine($"                obj.{propertyName} = {mappedPropertyName}.Value.Kind == DateTimeKind.Utc ? {mappedPropertyName}.Value.ToLocalTime() : {mappedPropertyName};");
			}
			else
            {
				sb.AppendLine($"                obj.{propertyName} = {mappedPropertyName};");
			}
            sb.AppendLine($"            }}");
		}

		sb.AppendLine("            return obj;");
        sb.AppendLine("        }");
        sb.AppendLine();

        // Метод для преобразования в свойства
        sb.AppendLine($"        public static FrozenDictionary<string, object> MapToProperties({className} obj)");
        sb.AppendLine("        {");
        sb.AppendLine("            var properties = new Dictionary<string, object>();");

        foreach (var property in properties)
        {
            var propertyName = property.Name;
			var mappedPropertyName = propertyName.ToSnakeCase();
			var propertyType = property.Type.ToDisplayString();
		
            if (propertyType.Equals("System.DateTime"))
            {
				sb.AppendLine($"            if (obj.{propertyName} != DateTime.MinValue)");
				sb.AppendLine($"                properties[\"{mappedPropertyName}\"] = obj.{propertyName}.Kind == DateTimeKind.Utc ? obj.{propertyName} : obj.{propertyName}.ToUniversalTime();");
			}
			else if (propertyType.Equals("System.DateTime?"))
			{
				sb.AppendLine($"            if (obj.{propertyName} is not null)");
				sb.AppendLine($"                properties[\"{mappedPropertyName}\"] = obj.{propertyName}.Value.Kind == DateTimeKind.Utc ? obj.{propertyName} : obj.{propertyName}.Value.ToUniversalTime();");
			}
			else
            {
				sb.AppendLine($"            if (obj.{propertyName} is not null)");
				sb.AppendLine($"                properties[\"{mappedPropertyName}\"] = obj.{propertyName};");
            }
        }

        sb.AppendLine("            return properties.ToFrozenDictionary();");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}