//using System;
//using System.Threading.Tasks;
//using Command.Implementation;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using Shouldly;

//namespace Command.Test.Implementation
//{
//  [TestClass]
//  public class CommandInOutAsyncFixture
//  {
//    private const bool isNotValid = false;

//    private const bool isValid = true;

//    [TestMethod]
//    public async Task ReturnFailResultWhenInputIsNotValid()
//    {
//      var inputValidationMock = new Mock<IValidator<string>>();
//      inputValidationMock.Setup(v => v.Validate(It.IsAny<string>())).Returns(isNotValid).Verifiable();

//      var command = Mock.CreateInstanceOf<CommandInOutAsync<string, string>>(
//        inputValidationMock.Object,
//        It.IsAny<Validator<string>>());
//      var result = await command.ExecuteAsync(It.IsAny<string>());

//      result.Status.ShouldBe(Status.Fail);
//      inputValidationMock.Verify();
//    }

//    [TestMethod]
//    public async Task ReturnFailResultWhenOutputIsNotValid()
//    {
//      var inputValidationMock = new Mock<IValidator<string>>();
//      var outputValidationMock = new Mock<IValidator<string>>();
//      inputValidationMock.Setup(v => v.Validate(It.IsAny<string>())).Returns(isValid).Verifiable();
//      outputValidationMock.Setup(v => v.Validate(It.IsAny<string>())).Returns(isNotValid).Verifiable();

//      var command = Mock.CreateInstanceOf<CommandInOutAsync<string, string>>(
//        m => m.Setup(c => c.OnExecuteAsync(It.IsAny<string>())).ReturnsAsync(
//          new Result<string>
//          {
//            Status = Status.Success
//          }),
//        inputValidationMock.Object,
//        outputValidationMock.Object);
//      var result = await command.ExecuteAsync(It.IsAny<string>());

//      result.Status.ShouldBe(Status.Fail);
//      inputValidationMock.Verify();
//      outputValidationMock.Verify();
//    }

//    [TestMethod]
//    public async Task ReturnSuccessResultWhenInputAndOutputIsValid()
//    {
//      var inputValidationMock = new Mock<IValidator<string>>();
//      var outputValidationMock = new Mock<IValidator<string>>();
//      inputValidationMock.Setup(v => v.Validate(It.IsAny<string>())).Returns(isValid).Verifiable();
//      outputValidationMock.Setup(v => v.Validate(It.IsAny<string>())).Returns(isValid).Verifiable();

//      var command = Mock.CreateInstanceOf<CommandInOutAsync<string, string>>(
//        m => m.Setup(c => c.OnExecuteAsync(It.IsAny<string>())).ReturnsAsync(
//          new Result<string>
//          {
//            Status = Status.Success
//          }),
//        inputValidationMock.Object,
//        outputValidationMock.Object);
//      var result = await command.ExecuteAsync(It.IsAny<string>());

//      result.Status.ShouldBe(Status.Success);
//      inputValidationMock.Verify();
//      outputValidationMock.Verify();
//    }

//    [TestMethod]
//    public void ThrownNullReferenceExceptionWhenOnExecuteReturnNullResult()
//    {
//      var inputValidationMock = new Mock<IValidator<string>>();
//      var outputValidationMock = new Mock<IValidator<string>>();
//      inputValidationMock.Setup(v => v.Validate(It.IsAny<string>())).Returns(isValid).Verifiable();

//      var command = Mock.CreateInstanceOf<CommandInOutAsync<string, string>>(
//        m => m.Setup(c => c.OnExecuteAsync(It.IsAny<string>()))
//              .ReturnsAsync((Result<string>)null),
//        inputValidationMock.Object,
//        outputValidationMock.Object);

//      Should.Throw<NullReferenceException>(
//               () => command.ExecuteAsync(It.IsAny<string>())).Message
//            .ShouldBe("The result of OnExecute can not be null.");
//    }

//    [TestMethod]
//    public void ThrownNullReferenceExceptionWhenOnExecuteReturnNullTask()
//    {
//      var inputValidationMock = new Mock<IValidator<string>>();
//      var outputValidationMock = new Mock<IValidator<string>>();
//      inputValidationMock.Setup(v => v.Validate(It.IsAny<string>())).Returns(isValid).Verifiable();

//      var command = Mock.CreateInstanceOf<CommandInOutAsync<string, string>>(
//        m => m.Setup(c => c.OnExecuteAsync(It.IsAny<string>()))
//              .Returns((Task<Result<string>>)null),
//        inputValidationMock.Object,
//        outputValidationMock.Object);

//      Should.Throw<NullReferenceException>(
//               () => command.ExecuteAsync(It.IsAny<string>())).Message
//            .ShouldBe("The task of OnExecute can not be null.");
//    }
//  }
//}


