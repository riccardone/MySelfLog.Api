namespace MySelfLog.Contracts.Api
{
    /// <summary>
    /// returns the schema for the given client and version
    /// </summary>
    public interface ISchemaProvider
    {
        /// <summary>
        /// Returns the schema for the given client
        /// </summary>
        /// <param name="clientName"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        string GetSchema(string clientName, string version);
    }
}
