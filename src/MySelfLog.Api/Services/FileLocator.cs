using System.IO;

namespace MySelfLog.Api.Services
{
    public class FileLocator : IResourceLocator
    {
        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }
    }
}
