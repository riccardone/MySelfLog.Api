using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using MySelfLog.Contracts.Api;

namespace MySelfLog.Api.Services
{
    public class SchemaProvider : ISchemaProvider
    {
        readonly IConfiguration _configuration;
        readonly IResourceLocator _fileLocator;

        public SchemaProvider(IConfiguration configuration, IResourceLocator fileLocator)
        {
            _configuration = configuration;
            _fileLocator = fileLocator;
        }

        /// <summary>
        /// Returns the schema for the given client and version
        /// throws an exception if the file is not found
        /// </summary>
        /// <param name="clientName"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public string GetSchema(string clientName, string version)
        {
            var schemaPathRoot = _configuration.GetSection("Schema")["PathRoot"];
            var schemaFileName = _configuration.GetSection("Schema")["File"];
            var filePath = $"{schemaPathRoot}/{clientName}/{version}/{schemaFileName}";

            if(_fileLocator.Exists(filePath))
                return _fileLocator.ReadAllText(filePath);

            if (string.IsNullOrWhiteSpace(schemaPathRoot) && string.IsNullOrWhiteSpace(schemaFileName))
                throw new Exception("invalid config settings for Schema:PathRoot and Schema:File");
            throw new FileNotFoundException("specified schema not found");
        }
    }
}

