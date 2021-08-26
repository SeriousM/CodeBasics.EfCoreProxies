using System.Threading.Tasks;

namespace CodeBasics.Command.Implementation
{
  public class EmptyValidator<T> : IValidator<T>, IInputValidator<T>, IOutputValidator<T>
  {
    private EmptyValidator()
    {
    }

    public static EmptyValidator<T> Instance { get; } = new EmptyValidator<T>();

    public Task<ValidationStatus> ValidateAsync(T value)
    {
      return Task.FromResult(ValidationStatus.Valid);
    }
  }
}
