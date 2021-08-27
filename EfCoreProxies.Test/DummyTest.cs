////using System.Collections.Generic;
////using Microsoft.EntityFrameworkCore;

////namespace MyNamespace
////{
////  public class TestItem { }

////  [CodeBasics.EfCoreProxies.EfCoreNavigationCollectionProxyGenerated]
////  public class TestA
////  {
////    public ICollection<TestItem> Items { get; set; }
////  }

////  public class Consumer
////  {
////    public void Call()
////    {
////      var instance = new TestA();
////      var dbSet = new DbSetImpl<TestA>();
////      dbSet.CallMe(instance.Items);
////    }
////  }
////}

////namespace Microsoft.EntityFrameworkCore
////{
////  public class DbSetImpl<T> : DbSet<T> { }

////  public abstract class DbSet<T>
////  {
////    public void CallMe(object o) { }
////  }
////}
