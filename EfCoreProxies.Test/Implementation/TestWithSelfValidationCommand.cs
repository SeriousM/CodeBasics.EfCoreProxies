using System;
using System.Threading.Tasks;
using CodeBasics.Command.Implementation;
using Microsoft.Extensions.Logging;

namespace CodeBasics.Command.Test.Implementation
{
  public class TestWithSelfValidationCommand : CommandInOutAsyncBase<string, int>, IInputValidator<string>, IOutputValidator<int>
  {
    public int IncrementValue { get; set; }
    public int InputValidatorCalled { get; set; }
    public int OutputValidatorCalled { get; set; }

    public TestWithSelfValidationCommand(ILogger<TestCommand> logger) : base(logger, null, null)
    {
      InputValidator = this;
      OutputValidator = this;
    }

    Task<ValidationStatus> IValidator<string>.ValidateAsync(string value)
    {
      InputValidatorCalled = ++IncrementValue;

      return Task.FromResult(ValidationStatus.Valid);
    }

    protected internal override Task<IResult<int>> OnExecuteAsync(string input)
    {
      return Task.FromResult(Result.Success(++IncrementValue));
    }

    Task<ValidationStatus> IValidator<int>.ValidateAsync(int value)
    {
      OutputValidatorCalled = ++IncrementValue;

      return Task.FromResult(ValidationStatus.Valid);
    }
  }
}
