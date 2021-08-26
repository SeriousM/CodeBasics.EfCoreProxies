using System;
using System.Threading.Tasks;
using CodeBasics.Command.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeBasics.Command.Test.Implementation
{
  [TestClass]
  public class CommandFixture
  {
    private IServiceProvider services;

    public ICommandFactory CommandFactory => services.GetRequiredService<ICommandFactory>();

    [TestInitialize]
    public void TestSetup()
    {
      services = new ServiceCollection()
                .AddCommand()
                .AddLogging(opts => opts.SetMinimumLevel(LogLevel.Trace).AddConsole())
                
                .AddScoped<TestCommand>()
                .AddScoped<TestWithSelfValidationCommand>()
                
                .BuildServiceProvider(true)
                .CreateScope()
                .ServiceProvider;
    }

    [TestMethod]
    public async Task Create_should_create_a_working_command()
    {
      // arrange
      var command = CommandFactory.CreateAsync<TestCommand, string, int>("1");

      // act
      var result = await command.ExecuteAsync();

      // assert
      Assert.IsTrue(result.WasSuccessful);
      Assert.AreEqual(2, result.Value);
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
    public async Task Execute_command_twice_should_throw()
    {
      // arrange
      var command = CommandFactory.CreateAsync<TestCommand, string, int>("1");
      await command.ExecuteAsync();

      // act / assert
      await Assert.ThrowsExceptionAsync<CommandExecutionException>(() => command.ExecuteAsync());
    }

    [TestMethod]
    public async Task InputValidation_fail_should_return_bad_result()
    {
      // arrange
      var command = CommandFactory.CreateAsync<TestCommand, string, int>("1");
      ((IValidatorSetter<string, int>)command).SetInputValidator(new ActionValidator<string>(_ => false));

      // act
      var result = await command.ExecuteAsync();

      // assert
      Assert.IsFalse(result.WasSuccessful);
      Assert.AreEqual(CommandExecutionStatus.PreValidationFailed, result.Status);
    }

    [TestMethod]
    public async Task OutputValidation_fail_should_return_bad_result()
    {
      // arrange
      var command = CommandFactory.CreateAsync<TestCommand, string, int>("1");
      ((IValidatorSetter<string, int>)command).SetOutputValidator(new ActionValidator<int>(_ => false));

      // act
      var result = await command.ExecuteAsync();

      // assert
      Assert.IsFalse(result.WasSuccessful);
      Assert.AreEqual(CommandExecutionStatus.PostValidationFalied, result.Status);
    }

    [TestMethod]
    public async Task Execution_failed_should_return_bad_result()
    {
      // arrange
      var command = CommandFactory.CreateAsync<TestCommand, string, int>("1");
      ((TestCommand)command).FailExecution = true;

      // act
      var result = await command.ExecuteAsync();

      // assert
      Assert.IsFalse(result.WasSuccessful);
      Assert.AreEqual(CommandExecutionStatus.ExecutionError, result.Status);
    }
    
    [TestMethod]
    public async Task Execution_throws_should_return_bad_result()
    {
      // arrange
      var command = CommandFactory.CreateAsync<TestCommand, string, int>("1");
      ((TestCommand)command).ThrowExecution = true;

      // act / assert
      var result = await command.ExecuteAsync();

      Assert.IsFalse(result.WasSuccessful);
      Assert.AreEqual(CommandExecutionStatus.ExecutionError, result.Status);
    }
    
    [TestMethod]
    public async Task Execution_returns_null_should_return_bad_result()
    {
      // arrange
      var command = CommandFactory.CreateAsync<TestCommand, string, int>("1");
      ((TestCommand)command).ThrowExecution = true;

      // act / assert
      var result = await command.ExecuteAsync();

      Assert.IsFalse(result.WasSuccessful);
      Assert.AreEqual(CommandExecutionStatus.ExecutionError, result.Status);
    }
    
    [TestMethod]
    public async Task SelfValidatingCommand_Execution_calls_input_and_output_validator()
    {
      // arrange
      var command = CommandFactory.CreateAsync<TestWithSelfValidationCommand, string, int>("1");
      var concreteCommand = (TestWithSelfValidationCommand)command;
      concreteCommand.IncrementValue = 23;

      // act
      var result = await command.ExecuteAsync();

      // assert
      Assert.AreEqual(24, concreteCommand.InputValidatorCalled);
      Assert.AreEqual(25, result.Value);
      Assert.AreEqual(26, concreteCommand.OutputValidatorCalled);
    }
  }
}