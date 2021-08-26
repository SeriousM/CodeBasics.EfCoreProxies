namespace CodeBasics.Command
{
  public enum CommandExecutionStatus
  {
    /// <summary>
    /// This value indicates that the evaluation of the command result is not properly implemented.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The command was executed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The command was not executed because the input parameter validation failed.
    /// </summary>
    PreValidationFailed,

    /// <summary>
    /// The command execution failed because the command implementation encountered an error.
    /// </summary>
    ExecutionError,

    /// <summary>
    /// The command was executed but the post validation failed.
    /// Use post validation only if you have though unit tests in place or the command does not persist data.
    /// </summary>
    PostValidationFalied
  }
}
