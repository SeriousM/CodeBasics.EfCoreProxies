namespace CodeBasics.Command
{
  internal interface ISetSetCommandInput<in TIn>
  {
    void SetInputParameter(TIn value);
  }
}
