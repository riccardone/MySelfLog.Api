namespace MySelfLog.Contracts.Api
{
    public interface IMessageSenderFactory
    {
        IMessageSender Build(string source);
    }
}
