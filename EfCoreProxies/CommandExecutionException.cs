using System;

namespace CodeBasics.Command
{
  public class CommandExecutionException : Exception
  {
    private readonly string stackTrace;

    internal CommandExecutionException(string message) : base(message)
    {
    }

    internal CommandExecutionException(string message, Exception innerException) : base(message, innerException)
    {
      stackTrace = innerException?.StackTrace;
    }

    public IResult CommandResult { get; internal set; }

    /// <inheritdoc />
    public override string StackTrace => stackTrace ?? base.StackTrace;
  }
}
