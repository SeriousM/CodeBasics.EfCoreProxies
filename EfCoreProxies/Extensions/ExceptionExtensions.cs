#nullable enable
using System;

namespace CodeBasics.Command.Extensions
{
  public static class ExceptionExtensions
  {
    public const string UserMessageKey = "UserMessage";

    public static T WithUserMessage<T>(this T self, string? userMessage) where T : Exception
    {
      self.Data[UserMessageKey] = userMessage;

      return self;
    }

    public static string? GetUserMessage(this Exception self)
    {
      var userMessage = self.Data[UserMessageKey]?.ToString();

      return userMessage;
    }
  }
}
