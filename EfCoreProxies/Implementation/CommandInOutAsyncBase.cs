using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CodeBasics.Command.Implementation
{
  public abstract class CommandInOutAsyncBase<TIn, TOut> : ICommandInOutAsync<TOut>, ISetSetCommandInput<TIn>, IValidatorSetter<TIn, TOut>, ISetCommandOptions
  {
    private readonly object syncRoot = new object();
    private bool executed;
    private TIn input;

    protected CommandInOutAsyncBase(
      ILogger logger,
      IInputValidator<TIn> inputValidator,
      IOutputValidator<TOut> outputValidator)
    {
      Logger = logger;
      InputValidator = inputValidator;
      OutputValidator = outputValidator;
    }

    protected ILogger Logger { get; }
    protected internal CommandOptions Options { get; private set; }
    
    /// <summary>
    /// The input validator. Can be overwritten in case the command itself wants to validate.
    /// </summary>
    protected IInputValidator<TIn> InputValidator { get; set; }
    
    /// <summary>
    /// The output validator. Can be overwritten in case the command itself wants to validate.
    /// </summary>
    protected IOutputValidator<TOut> OutputValidator { get; set; }

    async Task<IResult<TOut>> ICommandInOutAsync<TOut>.ExecuteAsync()
    {
      lock (syncRoot)
      {
        if (executed)
        {
          var message = $"The command of type '{GetType().FullName}' was already executed.";
          Logger.LogError(message);

          throw new CommandExecutionException(message);
        }

        executed = true;
      }

      using (Logger.BeginScope($"Command Execution: {GetType().FullName}"))
      {
        var preValidationResult = await preValidationAsync();
        if (!preValidationResult.WasSuccessful)
        {
          throwStatusAsExceptionIfEnabled(preValidationResult);
          return preValidationResult;
        }

        var commandExecutionResult = await executeCommandAsync();
        if (!commandExecutionResult.WasSuccessful)
        {
          throwStatusAsExceptionIfEnabled(commandExecutionResult);
          return commandExecutionResult;
        }

        var postValidationResult = await postValidationAsync(commandExecutionResult.Value);
        if (!postValidationResult.WasSuccessful)
        {
          throwStatusAsExceptionIfEnabled(postValidationResult);
          return postValidationResult;
        }

        return commandExecutionResult;
      }
    }

    private async Task<IResult<TOut>> preValidationAsync()
    {
      if (InputValidator == null)
      {
        Logger.LogDebug($"Pre-Validation of command '{GetType().FullName}' skipped because of missing input validator.");
        return Result.Success<TOut>(default);
      }

      try
      {
        var preValidationStatus = await InputValidator.ValidateAsync(input);
        if (!preValidationStatus.IsValid)
        {
          var preValidationFail = Result<TOut>.PreValidationFail($"Pre-Validation failed:\n{preValidationStatus.Message}");

          return preValidationFail;
        }

        Logger.LogDebug($"Pre-Validation of command '{GetType().FullName}' was successful.");

        return Result.Success<TOut>(default);
      }
      catch (Exception ex)
      {
        var preValidationFail = Result<TOut>.PreValidationFail("Pre-Validation resulted in an exception.", ex);

        return preValidationFail;
      }
    }

    private async Task<IResult<TOut>> executeCommandAsync()
    {
      IResult<TOut> commandExecutionResult;
      try
      {
        commandExecutionResult = await OnExecuteAsync(input);
      }
      catch (Exception ex)
      {
        commandExecutionResult = Result.ExecutionError<TOut>("Execution resulted in an exception.", ex);
      }

      if (commandExecutionResult == null)
      {
        throw new CommandExecutionException($"The result of {nameof(OnExecuteAsync)} can not be null.");
      }

      if (commandExecutionResult.Status == CommandExecutionStatus.Success)
      {
        Logger.LogDebug($"Execution of command '{GetType().FullName}' was successful.");
      }

      return commandExecutionResult;
    }

    private async Task<IResult<TOut>> postValidationAsync(TOut commandExecutionResultValue)
    {
      if (OutputValidator == null)
      {
        Logger.LogDebug($"Post-Validation of command '{GetType().FullName}' skipped because of missing output validator.");
        return Result.Success<TOut>(default);
      }

      try
      {
        var postValidationStatus = await OutputValidator.ValidateAsync(commandExecutionResultValue);
        if (!postValidationStatus.IsValid)
        {
          var postValidationFail = Result<TOut>.PostValidationFail($"Post-Validation failed:\n{postValidationStatus.Message}");

          return postValidationFail;
        }

        Logger.LogDebug($"Post-Validation of command '{GetType().FullName}' was successful.");

        return Result.Success<TOut>(default);
      }
      catch (Exception ex)
      {
        var postValidationFail = Result<TOut>.PostValidationFail("Post-Validation resulted in an exception.", ex);

        return postValidationFail;
      }
    }

    private void throwStatusAsExceptionIfEnabled(IResult result)
    {
      if (result.Status == CommandExecutionStatus.PreValidationFailed)
      {
        if (Options.ThrowOnPreValidationFail)
        {
          var message = $"PreValidation failed for command '{GetType().FullName}':\n{result.Message}";
          Logger.LogError(result.Exception, message);

          throw new CommandExecutionException(message, result.Exception) { CommandResult = result };
        }
      }

      if (result.Status == CommandExecutionStatus.ExecutionError)
      {
        if (Options.ThrowOnExecutionError)
        {
          var message = $"Execution failed for command '{GetType().FullName}':\n{result.Message}";
          Logger.LogError(result.Exception, message);

          throw new CommandExecutionException(message, result.Exception) { CommandResult = result };
        }
      }

      if (result.Status == CommandExecutionStatus.PostValidationFalied)
      {
        if (Options.ThrowOnPostValidationFail)
        {
          var message = $"PostValidation failed for command '{GetType().FullName}':\n{result.Message}";
          Logger.LogError(result.Exception, message);

          throw new CommandExecutionException(message, result.Exception) { CommandResult = result };
        }
      }
    }

    void ISetCommandOptions.SetCommandOptions(CommandOptions value) => Options = value;

    void ISetSetCommandInput<TIn>.SetInputParameter(TIn value) => input = value;

    void IValidatorSetter<TIn, TOut>.SetInputValidator(IInputValidator<TIn> validator) => InputValidator = validator;

    void IValidatorSetter<TIn, TOut>.SetOutputValidator(IOutputValidator<TOut> validator) => OutputValidator = validator;

    protected internal abstract Task<IResult<TOut>> OnExecuteAsync(TIn input);
  }
}
