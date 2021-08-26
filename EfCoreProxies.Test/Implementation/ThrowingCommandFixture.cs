using System;
using System.Threading.Tasks;
using CodeBasics.Command.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeBasics.Command.Test.Implementation
{
  [TestClass]
  public class ThrowingCommandFixture
  {
    private IServiceProvider services;

    public ICommandFactory CommandFactory => services.GetRequiredService<ICommandFactory>();

    [TestInitialize]
    public void TestSetup()
    {
      services = new ServiceCollection()
                .AddCommand(opts =>
                 {
                   opts.ThrowOnPreValidationFail = true;
                   opts.ThrowOnExecutionError = true;
                   opts.ThrowOnPostValidationFail = true;
                 })
                .AddLogging(opts => opts.SetMinimumLevel(LogLevel.Trace).AddConsole())
                .AddScoped<TestCommand>()
                .BuildServiceProvider(true)
                .CreateScope()
                .ServiceProvider;
    }

    [TestMethod]
    public void Create_command_null_input_should_throw()
    {
      // arrange
      string input = null;

      // act / assert
      Assert.ThrowsException<ArgumentNullException>(() => CommandFactory.CreateAsync<TestCommand, string, int>(input));
    }

    [TestMethod]
    public async Task InputValidation_fail_should_throw()
    {
      // arrange
      var command = CommandFactory.CreateAsync<TestCommand, string, int>("1");
      ((IValidatorSetter<string, int>)command).SetInputValidator(new ActionValidator<string>(_ => false));

      // act / assert
      var exception = await Assert.ThrowsExceptionAsync<CommandExecutionException>(() => command.ExecuteAsync());

      Assert.IsFalse(exception.CommandResult.WasSuccessful);
      Assert.AreEqual(CommandExecutionStatus.PreValidationFailed, exception.CommandResult.Status);
    }

    [TestMethod]
    public async Task OutputValidation_fail_should_throw()
    {
      // arrange
      var command = CommandFactory.CreateAsync<TestCommand, string, int>("1");
      ((IValidatorSetter<string, int>)command).SetOutputValidator(new ActionValidator<int>(_ => false));

      // act / assert
      var exception = await Assert.ThrowsExceptionAsync<CommandExecutionException>(() => command.ExecuteAsync());

      Assert.IsFalse(exception.CommandResult.WasSuccessful);
      Assert.AreEqual(CommandExecutionStatus.PostValidationFalied, exception.CommandResult.Status);
    }

    [TestMethod]
    public async Task Execution_failed_should_throw()
    {
      // arrange
      var command = CommandFactory.CreateAsync<TestCommand, string, int>("1");
      ((TestCommand)command).FailExecution = true;

      // act / assert
      var exception = await Assert.ThrowsExceptionAsync<CommandExecutionException>(() => command.ExecuteAsync());

      Assert.IsFalse(exception.CommandResult.WasSuccessful);
      Assert.AreEqual(CommandExecutionStatus.ExecutionError, exception.CommandResult.Status);
    }

    [TestMethod]
    public async Task Execution_throws_should_throw()
    {
      // arrange
      var command = CommandFactory.CreateAsync<TestCommand, string, int>("1");
      ((TestCommand)command).ThrowExecution = true;

      // act / assert
      var exception = await Assert.ThrowsExceptionAsync<CommandExecutionException>(() => command.ExecuteAsync());

      Assert.IsFalse(exception.CommandResult.WasSuccessful);
      Assert.AreEqual(CommandExecutionStatus.ExecutionError, exception.CommandResult.Status);
    }
    
    [TestMethod]
    public async Task Execution_returns_null_should_throw()
    {
      // arrange
      var command = CommandFactory.CreateAsync<TestCommand, string, int>("1");
      ((TestCommand)command).ThrowExecution = true;

      // act / assert
      var exception = await Assert.ThrowsExceptionAsync<CommandExecutionException>(() => command.ExecuteAsync());

      Assert.IsFalse(exception.CommandResult.WasSuccessful);
      Assert.AreEqual(CommandExecutionStatus.ExecutionError, exception.CommandResult.Status);
    }
  }
}
