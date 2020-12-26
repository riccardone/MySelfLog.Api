namespace MySelfLog.Api.Services
{
    public interface IResourceLocator
    {
        bool Exists(string path);

        string ReadAllText(string path);

    }
}
