using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing.MSTest;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CSharpTest = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<
  CodeBasics.EfCoreProxies.EfCoreAvoidNavigationCollectionProxyAnalyzer, CodeBasics.EfCoreProxies.EfCoreAvoidNavigationCollectionProxyAnalyzerCodeFixProvider, Microsoft.CodeAnalysis.Testing.Verifiers.MSTestVerifier>;

namespace CodeBasics.EfCoreProxies.Test
{
  [TestClass]
  public class EfCoreAvoidNavigationCollectionProxyAnalyzerCodeFixProviderFixture
  {
    private static DiagnosticResult diagnostic()
      => CodeFixVerifier<EfCoreAvoidNavigationCollectionProxyAnalyzer, EfCoreAvoidNavigationCollectionProxyAnalyzerCodeFixProvider>.Diagnostic();

    [TestMethod]
    public async Task File_with_normal_property_usage_should_not_report()
    {
      var badSource = @"
public class Consumer 
{
  public void Call()
  {
    var instance = new TestA();
    var name = instance.Name;
  }
}";

      var goodSource = @"
public class Consumer 
{
  public void Call()
  {
    var instance = new TestA();
    var name = instance.Name;
  }
}";
      
      await runTestAsync(badSource, goodSource, null);
    }

    [TestMethod]
    public async Task File_with_collection_usage_should_report()
    {
      var badSource = @"
public class Consumer 
{
  public void Call()
  {
    var instance = new TestA();
    instance.Items.ToArray();
  }
}";

      var goodSource = @"
public class Consumer 
{
  public void Call()
  {
    var instance = new TestA();
    instance.ItemsQuery().ToArray();
  }
}";
      
      var expectedDiagnostic = diagnostic()
                              .WithMessage("Use the ItemsQuery() method")
                              .WithSeverity(DiagnosticSeverity.Warning)
                              .WithSpan("/0/Test0.cs", 12, 14, 12, 19)
                              .WithArguments("Items");
      
      await runTestAsync(badSource, goodSource, expectedDiagnostic);
    }

    [TestMethod]
    public async Task File_with_chained_collection_usage_should_report()
    {
      var badSource = @"
public class Consumer 
{
  public void Call()
  {
    var instance = new TestA();
    var count = instance.Items.ToArray().Length;
  }
}";

      var goodSource = @"
public class Consumer 
{
  public void Call()
  {
    var instance = new TestA();
    var count = instance.ItemsQuery().ToArray().Length;
  }
}";

      var expectedDiagnostic = diagnostic()
                              .WithMessage("Use the ItemsQuery() method")
                              .WithSeverity(DiagnosticSeverity.Warning)
                              .WithSpan("/0/Test0.cs", 12, 26, 12, 31)
                              .WithArguments("Items");

      await runTestAsync(badSource, goodSource, expectedDiagnostic);
    }

    [TestMethod]
    public async Task File_with_delayed_collection_usage_should_report()
    {
      var badSource = @"
public class Consumer 
{
  public void Call()
  {
    var instance = new TestA();
    var collection = instance.Items;
    var count = collection.ToArray().Length;
  }
}";

      var goodSource = @"
public class Consumer 
{
  public void Call()
  {
    var instance = new TestA();
    var collection = instance.ItemsQuery();
    var count = collection.ToArray().Length;
  }
}";

      var expectedDiagnostic = diagnostic()
                              .WithMessage("Use the ItemsQuery() method")
                              .WithSeverity(DiagnosticSeverity.Warning)
                              .WithSpan("/0/Test0.cs", 12, 31, 12, 36)
                              .WithArguments("Items");

      await runTestAsync(badSource, goodSource, expectedDiagnostic);
    }

    private static async Task runTestAsync(string badSource, string goodSource, DiagnosticResult? expectedDiagnostic)
    {
      var usings = getUsingsDefinition();
      var attributeDefinition = EfCoreNavigationCollectionProxyMethodGenerator.GenerateAttribute();
      var partialClassDefinition = getTestClassDefinition();

      var delimiter = "\n\n";
      badSource = usings + delimiter + badSource + delimiter + attributeDefinition + delimiter + partialClassDefinition;
      goodSource = usings + delimiter + goodSource + delimiter + attributeDefinition + delimiter + partialClassDefinition;

      var test = new CSharpTest
      {
        TestCode = badSource,
        FixedCode = goodSource,
        TestState =
        {
          Sources =
          {
          },
          AdditionalFiles =
          {
            ("File1.txt", "Content without braces"),
          }
        }
      };

      if (expectedDiagnostic.HasValue)
      {
        test.ExpectedDiagnostics.Add(expectedDiagnostic.Value);
      }

      await test.RunAsync();
    }

    private static string getUsingsDefinition()
    {
      return @"
using System;
using System.Collections.Generic;
using System.Linq;";
    }

    private static string getTestClassDefinition()
    {
      return @"
public class TestItem {}

[CodeBasics.EfCoreProxies.EfCoreNavigationCollectionProxyGenerated]
public class TestA
{
  public ICollection<TestItem> Items { get; set; }
  public string Name { get; set; }

  public IQueryable<TestItem> ItemsQuery()
  {
    return null!;
  }
}";
    }
  }
}
