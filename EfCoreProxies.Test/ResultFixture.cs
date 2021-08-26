using CodeBasics.Command.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace CodeBasics.Command.Test
{
  [TestClass]
  public class ResultFixture
  {
    [TestMethod]
    public void ReturnFailStatusWhenResultIsConstruct()
    {
      var result = Result<object>.PostValidationFail("failed");
      result.Status.ShouldBe(CommandExecutionStatus.PostValidationFalied);
    }

    [TestMethod]
    public void ReturnInstanceOfMessagesWhenResultIsConstruct()
    {
      var result = Result<object>.PreValidationFail("failed");
      result.Message.ShouldNotBeNull();
    }
  }
}
