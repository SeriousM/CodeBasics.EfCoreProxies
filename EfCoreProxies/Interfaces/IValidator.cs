using System.Threading.Tasks;
using CodeBasics.Command.Implementation;

namespace CodeBasics.Command
{
  public interface IValidator<in T>
  {
    Task<ValidationStatus> ValidateAsync(T value);
  }
}