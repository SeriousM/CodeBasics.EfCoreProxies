using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CodeBasics.Command.Extensions;
using Microsoft.Extensions.Logging;

namespace CodeBasics.Command.Implementation
{
  public class DataAnnotationsValidator<T> : IValidator<T>, IInputValidator<T>, IOutputValidator<T>
  {
    public DataAnnotationsValidator(ILogger<DataAnnotationsValidator<T>> logger)
    {
      Logger = logger;
    }

    protected ILogger<DataAnnotationsValidator<T>> Logger { get; }

    public async Task<ValidationStatus> ValidateAsync(T value)
    {
      if (value == null)
      {
        var message = $"The value of type '{typeof(T)}' cannot be null.";
        Logger.LogError(message);

        return new ValidationStatus(false, message);
      }

      if (value is ICollection collection)
      {
        var failedValidations = new List<ValidationStatus>();

        foreach (var item in collection)
        {
          var result = validateItem(item);

          if (!result.IsValid)
          {
            failedValidations.Add(result);
          }
        }

        if (failedValidations.Any())
        {
          return new ValidationStatus(false, $"Following validations for '{typeof(T)}' failed:\n{string.Join("\n", failedValidations.Select(v => v.Message))}");
        }

        return ValidationStatus.Valid;
      }

      var validationResult = validateItem(value);
      if (!validationResult.IsValid)
      {
        return validationResult;
      }

      var onValidateResult = OnValidate(value);
      if (!onValidateResult.IsValid)
      {
        Logger.LogError($"Validation of '{typeof(T)}' failed in {nameof(OnValidate)} method.");
      }

      return onValidateResult;
    }

    private ValidationStatus validateItem(object value)
    {
      if (value == null)
      {
        var message = "The item to validate is null.";
        Logger.LogError(message);

        return new ValidationStatus(false, message);
      }

      var type = value.GetType();

      if (TypeExtensions.IsPrimitive(type)
       || type.GetTypeInfo().IsGenericType
       && type.GetGenericTypeDefinition() == typeof(Nullable<>))
      {
        return new ValidationStatus(true, $"Type '{type}' is primitive and is considered as valid.");
      }

      var validationResults = new List<ValidationResult>();

      // TODO: support for FluidValidation?
      var valid = Validator.TryValidateObject(
        value,
        new ValidationContext(value, null, null),
        validationResults,
        true);

      if (!valid)
      {
        var validationReport = reportValidationResult(type, validationResults.ToArray());

        return new ValidationStatus(false, validationReport);
      }

      Logger.LogDebug($"Validation of '{type}' succeeded.");

      return ValidationStatus.Valid;
    }

    private string reportValidationResult(Type type, ValidationResult[] validationResults)
    {
      var validationReportInvalid = new List<(string members, string status)>();
      foreach (var validationResult in validationResults)
      {
        var members = memberNamesOrPlaceholder(validationResult.MemberNames);

          validationReportInvalid.Add((members, validationResult.ErrorMessage));
      }

      var sb = new StringBuilder();
      sb.AppendLine($"Validation of '{type}' failed:");

      foreach (var (members, status) in validationReportInvalid)
      {
        sb.AppendLine($"{members}: {status}");
      }

      Logger.LogError($"Validation of '{type}' failed:\n{sb}");

      return sb.ToString();
    }

    private static string memberNamesOrPlaceholder(IEnumerable<string> memberNames)
    {
      var names = memberNames as string[] ?? memberNames.ToArray();
      if (names.Length == 0)
      {
        return "<object>";
      }

      return string.Join(", ", names);
    }

    protected internal virtual ValidationStatus OnValidate(T value)
    {
      return ValidationStatus.Valid;
    }
  }
}
