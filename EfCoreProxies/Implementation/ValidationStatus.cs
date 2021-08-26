namespace CodeBasics.Command.Implementation
{
  public class ValidationStatus
  {
    public bool IsValid { get; }
    public string Message { get; }

    public ValidationStatus(bool isValid, string message = "")
    {
      IsValid = isValid;
      Message = message;
    }

    public static readonly ValidationStatus Valid = new ValidationStatus(true);
  }
}