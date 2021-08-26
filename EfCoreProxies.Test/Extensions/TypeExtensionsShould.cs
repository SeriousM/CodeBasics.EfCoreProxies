using System;
using CodeBasics.Command.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace CodeBasics.Command.Test.Extensions
{
  [TestClass]
  public class TypeExtensionsShould
  {
    [TestMethod]
    public void ReturnTrueWhenCallIsPrimitiveForDateTimeType()
    {
      var stringType = DateTime.Now.GetType();
      TypeExtensions.IsPrimitive(stringType).ShouldBeTrue();
    }

    [TestMethod]
    public void ReturnTrueWhenCallIsPrimitiveForDecimalType()
    {
      var stringType = 1.2m.GetType();
      TypeExtensions.IsPrimitive(stringType).ShouldBeTrue();
    }

    [TestMethod]
    public void ReturnTrueWhenCallIsPrimitiveForDoubleType()
    {
      var stringType = 1.3d.GetType();
      TypeExtensions.IsPrimitive(stringType).ShouldBeTrue();
    }

    [TestMethod]
    public void ReturnTrueWhenCallIsPrimitiveForGuidType()
    {
      var guidType = Guid.NewGuid().GetType();
      TypeExtensions.IsPrimitive(guidType).ShouldBeTrue();
    }

    [TestMethod]
    public void ReturnTrueWhenCallIsPrimitiveForIntType()
    {
      var stringType = 1.GetType();
      TypeExtensions.IsPrimitive(stringType).ShouldBeTrue();
    }

    [TestMethod]
    public void ReturnTrueWhenCallIsPrimitiveForStringType()
    {
      var stringType = "".GetType();
      TypeExtensions.IsPrimitive(stringType).ShouldBeTrue();
    }
  }
}
