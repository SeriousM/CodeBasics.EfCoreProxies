﻿using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeBasics.EfCoreProxies.Test
{
  [TestClass]
  public class EfCoreNavigationCollectionProxyMethodGeneratorFixture
  {
    [TestMethod]
    public void Generator_with_correct_code_should_generate_partial_class()
    {
      var userSource = @"
using System;
using System.Collections.Generic;

namespace CodeBasics.EfCoreProxies.Test
{
  public class TestItemA { }
  public class TestItemB { }

  public partial class TestClass
  {
    public virtual ICollection<TestItemA> TestItems1 { get; set; }
    public virtual ICollection<TestItemB> TestItems2 { get; set; }
  }
}
";
      var comp = createCompilation(userSource);
      var newComp = runGenerators(comp, out _, new EfCoreNavigationCollectionProxyMethodGenerator());

      var newFile = newComp.SyntaxTrees
                           .Single(x => Path.GetFileName(x.FilePath).EndsWith(".NavigationCollectionProxy.cs"));

      Assert.IsNotNull(newFile);
      Assert.IsTrue(newFile.FilePath.EndsWith("TestClass.NavigationCollectionProxy.cs"));

      var generatedText = newFile.GetText().ToString();

      var expectedOutput = @"
// <auto-generated>
using System.ComponentModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CodeBasics.EfCoreProxies.Test
{
  [CodeBasics.EfCoreProxies.EfCoreNavigationCollectionProxyGenerated]
  partial class TestClass
  {
    /// <summary>
    /// Constructor for EntityFramework. Don't use it! You have been warned.
    /// </summary>
    [System.Obsolete(""For internal use only."")]
    public TestClass(DbContext context) : this()
    {
      this.Context = context;
    }

    /// <summary>
    /// Private property to support entity framework to set the context once the entity gets attached.
    /// </summary>
    private DbContext Context { get; set; }

    /// <summary>
    /// Gets a IQueryable for the <see cref=""TestClass.TestItems1""/> collection.
    /// Use this to avoid eager-load of all related entities.
    /// </summary>
    public IQueryable<CodeBasics.EfCoreProxies.Test.TestItemA> TestItems1Query()
    {
      if (Context is null)
      {
        return TestItems1.AsQueryable();
      }

      var entityEntry = Context.Entry(this);
      var collectionEntry = entityEntry.Collection(e => e.TestItems1);
      var query = collectionEntry.Query();

      return query;
    }

    /// <summary>
    /// Gets a IQueryable for the <see cref=""TestClass.TestItems2""/> collection.
    /// Use this to avoid eager-load of all related entities.
    /// </summary>
    public IQueryable<CodeBasics.EfCoreProxies.Test.TestItemB> TestItems2Query()
    {
      if (Context is null)
      {
        return TestItems2.AsQueryable();
      }

      var entityEntry = Context.Entry(this);
      var collectionEntry = entityEntry.Collection(e => e.TestItems2);
      var query = collectionEntry.Query();

      return query;
    }
  }
}".TrimStart();

      Assert.AreEqual(expectedOutput, generatedText);
    }

    private static Compilation createCompilation(string source)
    {
      return CSharpCompilation.Create("compilation",
        new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp11)) },
        new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) /* additional assemblies to load */ },
        new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    }

    private static GeneratorDriver createDriver(params ISourceGenerator[] generators)
    {
      return CSharpGeneratorDriver.Create(generators);
    }

    private static Compilation runGenerators(Compilation compilation, out ImmutableArray<Diagnostic> diagnostics, params ISourceGenerator[] generators)
    {
      createDriver(generators).RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out diagnostics);

      return newCompilation;
    }
  }
}
