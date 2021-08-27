using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CSharpTest = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixTest<
  CodeBasics.EfCoreProxies.EfCoreAvoidNavigationCollectionProxyAnalyzer, CodeBasics.EfCoreProxies.EfCoreAvoidNavigationCollectionProxyAnalyzerCodeFixProvider, Microsoft.CodeAnalysis.Testing.Verifiers.MSTestVerifier>;

namespace CodeBasics.EfCoreProxies.Test
{
  [TestClass]
  public class EfCoreAvoidNavigationCollectionProxyAnalyzerFixture
  {
    private static DiagnosticResult diagnostic()
      => AnalyzerVerifier<EfCoreAvoidNavigationCollectionProxyAnalyzer, CSharpTest, MSTestVerifier>.Diagnostic();

    [TestMethod]
    public async Task File_with_only_namespace_should_not_report()
    {
      var source = @"
namespace MyNamespace { }";

      await runTestAsync(source, null);
    }

    [TestMethod]
    public async Task File_with_empty_class_should_not_report()
    {
      var source = @"
namespace MyNamespace
{
  public class TestA { }
}";

      await runTestAsync(source, null);
    }

    [TestMethod]
    public async Task File_with_simple_property_should_not_report()
    {
      var source = @"
namespace MyNamespace
{
  public class TestA
  {
    public string Name { get; set; }
  }
}";

      await runTestAsync(source, null);
    }

    [TestMethod]
    public async Task File_with_one_collection_property_should_not_report()
    {
      var source = @"
using System.Collections.Generic;

namespace MyNamespace
{
  public class TestItem {}

  public class TestA
  {
    public ICollection<TestItem> Items { get; set; }
  }
}";

      await runTestAsync(source, null);
    }

    [TestMethod]
    public async Task File_with_collection_usage_but_without_attribute_should_not_report()
    {
      var source = @"
using System.Collections.Generic;
using System.Linq;

namespace MyNamespace
{
  public class TestItem {}

  public class TestA
  {
    public ICollection<TestItem> Items { get; set; }
  }

  public class Consumer 
  {
    public void Call()
    {
      var instance = new TestA();
      var count = instance.Items.ToArray().Length;
    }
  }
}";

      await runTestAsync(source, null);
    }

    [TestMethod]
    public async Task File_with_assignment_to_collection_should_not_report()
    {
      var source = @"
using System.Collections.Generic;
using System.Linq;

namespace MyNamespace
{
  public class TestItem {}

  [CodeBasics.EfCoreProxies.EfCoreNavigationCollectionProxyGenerated]
  public class TestA
  {
    public ICollection<TestItem> Items { get; set; }
  }

  public class Consumer 
  {
    public void Call()
    {
      var instance = new TestA();
      instance.Items = new List<TestItem>();
    }
  }
}";

      await runTestAsync(source, null);
    }

    [TestMethod]
    public async Task File_with_nameof_of_collection_name_usage_should_not_report()
    {
      var source = @"
using System.Collections.Generic;
using System.Linq;

namespace MyNamespace
{
  public class TestItem {}

  [CodeBasics.EfCoreProxies.EfCoreNavigationCollectionProxyGenerated]
  public class TestA
  {
    public ICollection<TestItem> Items { get; set; }
  }

  public class Consumer 
  {
    public void Call()
    {
      var instance = new TestA();
      var name = nameof(instance.Items);
    }
  }
}";

      await runTestAsync(source, null);
    }

    [TestMethod]
    public async Task File_with_usage_of_collection_property_within_a_DbContext_should_not_report()
    {
      var source = @"
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.EntityFrameworkCore
{
  public class DbContext {}
}

namespace MyNamespace
{
  public class TestItem {}

  [CodeBasics.EfCoreProxies.EfCoreNavigationCollectionProxyGenerated]
  public class TestA
  {
    public ICollection<TestItem> Items { get; set; }
  }

  public class Consumer : Microsoft.EntityFrameworkCore.DbContext
  {
    public void Call()
    {
      var instance = new TestA();
      var tmp = instance.Items;
    }
  }
}";

      await runTestAsync(source, null);
    }

    [TestMethod]
    public async Task File_with_collection_usage_via_DbSet_extensions_should_not_report()
    {
      var source = @"
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MyNamespace
{
  public class TestItem { }

  [CodeBasics.EfCoreProxies.EfCoreNavigationCollectionProxyGenerated]
  public class TestA
  {
    public ICollection<TestItem> Items { get; set; }
  }

  public class Consumer
  {
    public void Call()
    {
      var instance = new TestA();
      var dbSet = new DbSetImpl<TestA>();
      dbSet.CallMe(instance.Items);
    }
  }
}

namespace Microsoft.EntityFrameworkCore
{
  public class DbSetImpl<T> : DbSet<T> { }

  public abstract class DbSet<T>
  {
    public void CallMe(object o) { }
  }
}";

      await runTestAsync(source, null);
    }

    [TestMethod]
    public async Task File_with_collection_usage_via_Queryable_extensions_should_not_report()
    {
      var source = @"
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MyNamespace
{
  public class TestItem {}

  [CodeBasics.EfCoreProxies.EfCoreNavigationCollectionProxyGenerated]
  public class TestA
  {
    public ICollection<TestItem> Items { get; set; }
  }

  public class Consumer
  {
    public void Call()
    {
      var instance = new TestA();
      instance.CallMe(x => x.Items);
    }
  }
}

namespace System.Linq
{
  public static class Queryable
  {
    public static void CallMe(this MyNamespace.TestA self, Expression<Func<MyNamespace.TestA, ICollection<MyNamespace.TestItem>>> myExpression) { }
  }
}";

      await runTestAsync(source, null);
    }

    [TestMethod]
    public async Task File_with_collection_usage_via_direct_Queryable_extensions_should_not_report()
    {
      var source = @"
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MyNamespace
{
  public class TestItem { }

  [CodeBasics.EfCoreProxies.EfCoreNavigationCollectionProxyGenerated]
  public class TestA
  {
    public ICollection<TestItem> Items { get; set; }
  }

  public class Consumer
  {
    public void Call()
    {
      var instance = new TestA();
      Queryable.CallMe(instance, x => x.Items);
    }
  }
}

namespace System.Linq
{
  public static class Queryable
  {
    public static void CallMe(this MyNamespace.TestA self, Expression<Func<MyNamespace.TestA, ICollection<MyNamespace.TestItem>>> myExpression) { }
  }
}";

      await runTestAsync(source, null);
    }

    [TestMethod]
    public async Task File_with_collection_usage_via_EntityFrameworkQueryableExtensions_extensions_should_not_report()
    {
      var source = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace MyNamespace
{
  public class TestItem { }

  [CodeBasics.EfCoreProxies.EfCoreNavigationCollectionProxyGenerated]
  public class TestA
  {
    public ICollection<TestItem> Items { get; set; }
  }

  public class Consumer
  {
    public void Call()
    {
      var instance = new TestA();
      instance.CallMe(x => x.Items);
    }
  }
}

namespace Microsoft.EntityFrameworkCore
{
  public static class EntityFrameworkQueryableExtensions
  {
    public static void CallMe(this MyNamespace.TestA self, Expression<Func<MyNamespace.TestA, ICollection<MyNamespace.TestItem>>> myExpression) { }
  }
}";

      await runTestAsync(source, null);
    }

    [TestMethod]
    public async Task File_with_collection_usage_should_report()
    {
      var source = @"
using System.Collections.Generic;
using System.Linq;

namespace MyNamespace
{
  public class TestItem {}

  [CodeBasics.EfCoreProxies.EfCoreNavigationCollectionProxyGenerated]
  public class TestA
  {
    public ICollection<TestItem> Items { get; set; }
  }

  public class Consumer 
  {
    public void Call()
    {
      var instance = new TestA();
      instance.Items.ToArray();
    }
  }
}";

      var expectedDiagnostic = diagnostic()
                              .WithMessage("Use the ItemsQuery() method")
                              .WithSeverity(DiagnosticSeverity.Warning)
                              .WithSpan("/0/Test1.cs", 20, 16, 20, 21)
                              .WithArguments("Items");

      await runTestAsync(source, expectedDiagnostic);
    }

    [TestMethod]
    public async Task File_with_chained_collection_usage_should_report()
    {
      var source = @"
using System.Collections.Generic;
using System.Linq;

namespace MyNamespace
{
  public class TestItem {}

  [CodeBasics.EfCoreProxies.EfCoreNavigationCollectionProxyGenerated]
  public class TestA
  {
    public ICollection<TestItem> Items { get; set; }
  }

  public class Consumer 
  {
    public void Call()
    {
      var instance = new TestA();
      var count = instance.Items.ToArray().Length;
    }
  }
}";

      var expectedDiagnostic = diagnostic()
                              .WithMessage("Use the ItemsQuery() method")
                              .WithSeverity(DiagnosticSeverity.Warning)
                              .WithSpan("/0/Test1.cs", 20, 28, 20, 33)
                              .WithArguments("Items");

      await runTestAsync(source, expectedDiagnostic);
    }

    [TestMethod]
    public async Task File_with_delayed_collection_usage_should_report()
    {
      var source = @"
using System.Collections.Generic;
using System.Linq;

namespace MyNamespace
{
  public class TestItem {}

  [CodeBasics.EfCoreProxies.EfCoreNavigationCollectionProxyGenerated]
  public class TestA
  {
    public ICollection<TestItem> Items { get; set; }
  }

  public class Consumer 
  {
    public void Call()
    {
      var instance = new TestA();
      var collection = instance.Items;
      var count = collection.ToArray().Length;
    }
  }
}";

      var expectedDiagnostic = diagnostic()
                              .WithMessage("Use the ItemsQuery() method")
                              .WithSeverity(DiagnosticSeverity.Warning)
                              .WithSpan("/0/Test1.cs", 20, 33, 20, 38)
                              .WithArguments("Items");

      await runTestAsync(source, expectedDiagnostic);
    }

    [TestMethod]
    public async Task File_with_normal_property_usage_should_not_report()
    {
      var source = @"
using System.Collections.Generic;
using System.Linq;

namespace MyNamespace
{
  public class TestItem {}

  [CodeBasics.EfCoreProxies.EfCoreNavigationCollectionProxyGenerated]
  public class TestA
  {
    public ICollection<TestItem> Items { get; set; }
    public string Name { get; set; }
  }

  public class Consumer 
  {
    public void Call()
    {
      var instance = new TestA();
      var name = instance.Name;
    }
  }
}";

      await runTestAsync(source, null);
    }

    private static async Task runTestAsync(string source, DiagnosticResult? expectedDiagnostic)
    {
      var attributeDefinition = EfCoreNavigationCollectionProxyMethodGenerator.GenerateAttributeHeader()
                              + EfCoreNavigationCollectionProxyMethodGenerator.GenerateAttribute();

      var test = new CSharpTest
      {
        TestState =
          {
            Sources =
            {
              attributeDefinition,
              source
            },
            AdditionalFiles =
            {
              ("File1.txt", "Content without braces"),
            }
          }
      };

      if (expectedDiagnostic.HasValue)
      {
        test.TestState.ExpectedDiagnostics.Add(expectedDiagnostic.Value);
      }

      await test.RunAsync();
    }
  }
}
