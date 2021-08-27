using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CodeBasics.EfCoreProxies
{
  [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EfCoreAvoidNavigationCollectionProxyAnalyzerCodeFixProvider)), Shared]
  public class EfCoreAvoidNavigationCollectionProxyAnalyzerCodeFixProvider : CodeFixProvider
  {
    // The name as it will appear in the light bulb menu
    private const string title = "Avoid collection navigation property usage";

    // The list of rules the code fix can handle
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(EfCoreAvoidNavigationCollectionProxyAnalyzer.DiagnosticId);

    public sealed override FixAllProvider GetFixAllProvider()
    {
      // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
      return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
      var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
      
      var diagnostic = context.Diagnostics.First();
      var diagnosticSpan = diagnostic.Location.SourceSpan;
      
      // Find the type declaration identified by the diagnostic.
      var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MemberAccessExpressionSyntax>().First();
      
      // Register a code action that will invoke the fix.
      context.RegisterCodeFix(
          CodeAction.Create(
              title: title,
              createChangedDocument: c => fixAsync(context.Document, declaration, diagnosticSpan, c),
              equivalenceKey: title),
          diagnostic);
    }

    private static async Task<Document> fixAsync(Document document, MemberAccessExpressionSyntax memberAccess, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
      var fullCode = await document.GetTextAsync(cancellationToken);
      
      var problematicCode = fullCode.GetSubText(diagnosticSpan).ToString();
      var fixedSegment = problematicCode.Replace(memberAccess.Name.ToString(), $"{memberAccess.Name}Query()");

      var fixedCode = fullCode.Replace(diagnosticSpan, fixedSegment);
      var fixedDocument = document.WithText(fixedCode);

      return fixedDocument;

      ////var expressionSyntax = SyntaxFactory.ParseExpression("ItemsQuery()");

      ////memberAccess.Expression.ReplaceSyntax(
      ////  new[] { memberAccess },
      ////  (original, rewrite) => expressionSyntax,
      ////  new SyntaxToken[0],
      ////  (original, replacement) => replacement,
      ////  new SyntaxTrivia[0],
      ////  (original, replacement) => replacement);

      //////var fullParameterNode = root.FindNode(diagnostic.Location.SourceSpan, false) as ParameterSyntax;

      //////memberAccess.Expression.ReplaceNode(memberAccess, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(new ))

      ////var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);

      ////return document;

      ////// Create a new list of arguments with System.StringComparison.Ordinal
      ////var arguments = invocationExpr.ArgumentList.AddArguments(
      ////    Argument(
      ////            MemberAccessExpression(
      ////                SyntaxKind.SimpleMemberAccessExpression,
      ////                QualifiedName(IdentifierName("System"), IdentifierName("StringComparison")),
      ////                IdentifierName("Ordinal"))));

      ////// Indicate to format the list with the current coding style
      ////var formattedLocal = arguments.WithAdditionalAnnotations(Formatter.Annotation);

      ////// Replace the old local declaration with the new local declaration.
      ////var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
      ////var newRoot = oldRoot.ReplaceNode(invocationExpr.ArgumentList, formattedLocal);

      ////return document.WithSyntaxRoot(newRoot);
    }
  }
}
