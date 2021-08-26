using Microsoft.Extensions.Options;

namespace CodeBasics.Command
{
  public class CommandOptions : IOptions<CommandOptions>
  {
    public bool ThrowOnPreValidationFail { get; set; }
    public bool ThrowOnExecutionError { get; set; }
    public bool ThrowOnPostValidationFail { get; set; }

    CommandOptions IOptions<CommandOptions>.Value => this;

    internal static void DefaultSettings(CommandOptions options)
    {
      // do nothing, everything stays "false" as default
    }
  }
}
