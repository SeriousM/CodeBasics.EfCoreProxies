using System;
using System.Reflection;

namespace CodeBasics.Command.Extensions
{
  internal static class TypeExtensions
  {
    public static bool IsPrimitive(Type that)
    {
      return that.GetTypeInfo().IsPrimitive
          || that == typeof(string)
          || that == typeof(decimal)
          || that == typeof(DateTime)
          || that == typeof(Guid);
    }
  }
}
