using CodeBasics.Command.Implementation;

namespace CodeBasics.Command
{
  public interface ICommandFactory
  {
    ICommandInOutAsync<TOut> CreateAsync<TCommand, TIn, TOut>(TIn input) where TCommand : CommandInOutAsyncBase<TIn, TOut>;
  }
}
