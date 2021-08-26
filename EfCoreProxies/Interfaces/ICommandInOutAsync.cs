using System.Threading.Tasks;

namespace CodeBasics.Command
{
  public interface ICommandInOutAsync<TOut>
  {
    Task<IResult<TOut>> ExecuteAsync();
  }
}
