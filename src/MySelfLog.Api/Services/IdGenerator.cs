using System;
using MySelfLog.Contracts.Api;

namespace MySelfLog.Api.Services
{
    public class IdGenerator : IIdGenerator
    {
        public string GenerateId(string prefix)
        {
            return $"{prefix}{Guid.NewGuid()}";
        }
    }
}
