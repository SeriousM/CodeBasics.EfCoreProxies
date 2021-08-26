#nullable enable
using System;
using CodeBasics.Command.Extensions;

namespace CodeBasics.Command.Implementation
{
  public static class Result
  {
    public static IResult<TValue> Success<TValue>(TValue value)
    {
      return Result<TValue>.Success(value);
    }

    public static IResult<TValue> ExecutionError<TValue>(string message, Exception? exception = null, string? userMessage = null)
    {
      return Result<TValue>.ExecutionError(message, exception, userMessage);
    }
  }

  public sealed class Result<TValue> : IResult<TValue>
  {
    private Result()
    {
    }

    public string Message { get; private set; } = default!;
    
    public string? UserMessage { get; private set; }

    public Exception? Exception { get; private set; }

    public CommandExecutionStatus Status { get; private set; }

    // TODO: annotate with [System.Diagnostics.CodeAnalysis.NotNullWhen(WasSuccessful)] once .netstandard21 or greater
    public TValue Value { get; private set; } = default!;

    public bool WasSuccessful => Status == CommandExecutionStatus.Success;

    internal static IResult<TValue> PreValidationFail(string message, Exception? exception = null, string? userMessage = null)
    {
      exception?.WithUserMessage<Exception>(userMessage);

      return new Result<TValue> { Message = message, Status = CommandExecutionStatus.PreValidationFailed, Exception = exception, UserMessage = userMessage };
    }

    internal static IResult<TValue> PostValidationFail(string message, Exception? exception = null, string? userMessage = null)
    {
      exception?.WithUserMessage<Exception>(userMessage);

      return new Result<TValue> { Message = message, Status = CommandExecutionStatus.PostValidationFalied, Exception = exception, UserMessage = userMessage };
    }

    internal static IResult<TValue> ExecutionError(string message, Exception? exception, string? userMessage = null)
    {
      exception?.WithUserMessage<Exception>(userMessage);

      return new Result<TValue> { Status = CommandExecutionStatus.ExecutionError, Message = message, Exception = exception, UserMessage = userMessage };
    }

    internal static IResult<TValue> Success(TValue result)
    {
      return new Result<TValue> { Status = CommandExecutionStatus.Success, Value = result };
    }
  }
}
