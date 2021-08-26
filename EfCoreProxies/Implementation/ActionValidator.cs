using System;
using System.Threading.Tasks;

namespace CodeBasics.Command.Implementation
{
  public class ActionValidator<T> : IValidator<T>, IInputValidator<T>, IOutputValidator<T>
  {
    private readonly Func<T, Task<bool>> validatorAsyncFunc;

    public ActionValidator(Func<T, Task<bool>> validatorAsyncFunc)
    {
      this.validatorAsyncFunc = validatorAsyncFunc ?? throw new ArgumentNullException(nameof(validatorAsyncFunc));
    }

    public ActionValidator(Func<T, bool> validatorFunc)
    {
      if (validatorFunc == null)
      {
        throw new ArgumentNullException(nameof(validatorFunc));
      }

      validatorAsyncFunc = value => Task.FromResult(validatorFunc(value));
    }

    public async Task<ValidationStatus> ValidateAsync(T value)
    {
      var valid = await validatorAsyncFunc(value);

      return new ValidationStatus(valid);
    }
  }
}
