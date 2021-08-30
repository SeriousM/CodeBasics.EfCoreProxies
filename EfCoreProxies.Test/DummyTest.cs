////using System;
////using System.Collections;
////using System.Collections.Generic;
////using System.Linq;
////using System.Linq.Expressions;
////using Microsoft.EntityFrameworkCore;

////namespace MyNamespace
////{
////  public class TestItem { }

////  [CodeBasics.EfCoreProxies.EfCoreNavigationCollectionProxyGenerated]
////  public class TestA
////  {
////    public ICollection<TestB> Items { get; set; }
////  }

////  [CodeBasics.EfCoreProxies.EfCoreNavigationCollectionProxyGenerated]
////  public class TestB
////  {
////    public ICollection<TestItem> Items { get; set; }
////  }

////  public class DbContext
////  {
////    public DbSetImpl<TestA> TestAs { get; set; }
////  }

////  public class Consumer
////  {
////    public void Call()
////    {
////      var context = new DbContext();

////      var items = context.TestAs
////                         .Where(t => t.Items.Any(x => true))
////                         .Select(x => 1)
////                         .ToArray();
////    }
////  }
////}

////namespace Microsoft.EntityFrameworkCore
////{
////  public class DbSetImpl<T> : DbSet<T> { }

////  public abstract class DbSet<T> : IQueryable<T>
////  {
////    public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();
////    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
////    public Type ElementType { get; }
////    public Expression Expression { get; }
////    public IQueryProvider Provider { get; }
////  }
////}
