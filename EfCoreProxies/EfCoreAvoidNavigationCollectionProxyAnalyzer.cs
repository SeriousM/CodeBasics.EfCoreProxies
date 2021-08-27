using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CodeBasics.EfCoreProxies
{
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class EfCoreAvoidNavigationCollectionProxyAnalyzer : DiagnosticAnalyzer
  {
    // https://www.meziantou.net/writing-a-roslyn-analyzer.htm

    // Metadata of the analyzer
    public const string DiagnosticId = "EFPA01";

    // You could use LocalizedString but it's a little more complicated for this sample
    private const string title = "Collection navigation properties should not be used for querying";
    private const string messageFormat = "Use the {0}Query() method";
    private const string description = "Using collection navigation properties will eager-load all related entities into memory. This could result in degraded performance.";
    private const string category = "Usage";

    private static readonly DiagnosticDescriptor rule = new DiagnosticDescriptor(DiagnosticId, title, messageFormat, category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: description);

    // Register the list of rules this DiagnosticAnalizer supports
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
      context.EnableConcurrentExecution();

      // The AnalyzeNode method will be called for each InvocationExpression of the Syntax tree
      context.RegisterSyntaxNodeAction(analyzeNode, SyntaxKind.SimpleMemberAccessExpression);
    }

    private static void analyzeNode(SyntaxNodeAnalysisContext context)
    {
      // NOTE: verify in the fast-to-check to expensive-to-check order.
      // meaning that check if the property has a special name is faster than searching for base class attributes.
      // this is important as that we get called many many many times.

      // contains all the composition information of the assembly and it's references
      var compilation = context.Compilation;

      // instance.collection.method -> memberAccessExpr.Expression
      // instance.collection; -> memberAccessExpr
      var memberAccessExpr = (MemberAccessExpressionSyntax)context.Node;

      // skip if the property is about to be assigned (assign new value to it)
      if (memberAccessExpr.Parent is AssignmentExpressionSyntax)
        return;
      
      // skip "nameof" usages
      if (isNameOf(context.SemanticModel, memberAccessExpr))
        return;

      // skip if the collection is referenced in a linq-query
      if (isWithinLinqQuery(context.SemanticModel, memberAccessExpr))
        return;

      // skip classes that are allowed to use the collection navigation properties
      if (isWithinAllowedClasses(context.SemanticModel, memberAccessExpr))
        return;

      // skip if the property is called by queryable/ef extension methods
      if (isCalledByAllowedExtensionClasses(context.SemanticModel, memberAccessExpr))
        return;

      // skip if the call is in a lambda expression and the class is allowed
      if (isCalledByAllowedExtensionClassesInLambda(context.SemanticModel, memberAccessExpr))
        return;

      // lets see if the accessed member is a property
      var propertySymbol = context.SemanticModel.GetSymbolInfo(memberAccessExpr).Symbol as IPropertySymbol;
      if (propertySymbol == null)
        return;

      // skip if the property is not a collection
      if (propertySymbol.Type.Name != "ICollection")
        return;

      // skip if the properties containing type (the entity) is extended by our code generator
      var symbolType = propertySymbol.ContainingType;
      if (!hasGeneratorAttribute(symbolType))
        return;

      // alright, we're certain that the found member access is a violation
      var memberName = propertySymbol.Name;

      // get the location of the exact property usage
      var location = memberAccessExpr.DescendantNodes()
                                     .OfType<IdentifierNameSyntax>()
                                     .FirstOrDefault(n => n.Identifier.Text == memberName)?.GetLocation()
                  ?? memberAccessExpr.GetLocation();

      var diagnostic = Diagnostic.Create(rule, location, memberName);
      context.ReportDiagnostic(diagnostic);
    }

    private static bool isCalledByAllowedExtensionClasses(SemanticModel semanticModel, SyntaxNode memberAccessExpr)
    {
      var surroundingInvocation = findParentOf<InvocationExpressionSyntax>(memberAccessExpr);
      var memberAccessExpressionSyntax = surroundingInvocation?.ChildNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault();

      if (memberAccessExpressionSyntax is null)
        return false;

      var methodSymbol = semanticModel.GetSymbolInfo(memberAccessExpressionSyntax).Symbol as IMethodSymbol;
      var methodContainingType = methodSymbol?.ContainingType;

      if (methodContainingType is null)
        return false;

      if (methodContainingType.Name == "Queryable"
       && methodContainingType.ContainingNamespace.ToString() == "System.Linq")
        return true;
      
      if (methodContainingType.Name is "EntityFrameworkQueryableExtensions" or "DbSet"
       && methodContainingType.ContainingNamespace.ToString() == "Microsoft.EntityFrameworkCore")
        return true;

      return false;
    }

    private static bool isCalledByAllowedExtensionClassesInLambda(SemanticModel semanticModel, MemberAccessExpressionSyntax memberAccessExpr)
    {
      var lambdaExpressionSyntax = findParentOf<SimpleLambdaExpressionSyntax>(memberAccessExpr);

      if (lambdaExpressionSyntax is null)
        return false;

      return isCalledByAllowedExtensionClasses(semanticModel, lambdaExpressionSyntax);
    }

    private static bool hasGeneratorAttribute(INamedTypeSymbol symbolType)
    {
      var attributes = symbolType.GetAttributes();

      if (attributes.Length == 0)
        return false;

      if (!attributes.Any(a => a.AttributeClass?.Name == "EfCoreNavigationCollectionProxyGeneratedAttribute"
                            && a.AttributeClass?.ContainingNamespace?.ToString() == "CodeBasics.EfCoreProxies"))
        return false;

      return true;
    }

    private static bool isNameOf(SemanticModel semanticModel, SyntaxNode memberAccessExpr)
    {
      var invocationExpressionSyntax = findParentOf<InvocationExpressionSyntax>(memberAccessExpr);

      if (invocationExpressionSyntax is null) return false;

      var operation = semanticModel.GetOperation(invocationExpressionSyntax);
      var isNameOfOperation = operation is INameOfOperation;

      return isNameOfOperation;
    }

    private static bool isWithinLinqQuery(SemanticModel semanticModel, SyntaxNode memberAccessExpr)
    {
      var queryBodySyntax = findParentOf<QueryBodySyntax>(memberAccessExpr);
      
      if (queryBodySyntax is not null)
        return true;

      return false;
    }

    private static bool isWithinAllowedClasses(SemanticModel semanticModel, SyntaxNode memberAccessExpr)
    {
      var classDeclarationSyntax = findParentOf<ClassDeclarationSyntax>(memberAccessExpr);

      if (classDeclarationSyntax is null)
        return false;
      
      var classSymbol = semanticModel.GetDeclaredSymbol(classDeclarationSyntax);

      if (classSymbol == null)
        return false;

      // check if the base class (one level) is dbcontext
      if (classSymbol.BaseType?.Name == "DbContext" && classSymbol.BaseType?.ContainingNamespace.ToString() == "Microsoft.EntityFrameworkCore")
        return true;

      // check if the class is the generated one (might skip "bad usages" in the entities itself, but they are usually fine - except for repository patterns)
      if (hasGeneratorAttribute(classSymbol))
        return true;

      return false;
    }

    private static T? findParentOf<T>(SyntaxNode child) where T : SyntaxNode
    {
      for (var current = child; current != null; current = current.Parent)
      {
        if (current is T x)
          return x;
      }

      return null;
    }
  }
}
