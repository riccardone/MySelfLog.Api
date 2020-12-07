namespace MySelfLog.Contracts.Api
{
    public interface IPayloadValidator
    {
        PayloadValidationResult Validate(string schema, object value);
    }
}
