using System;

namespace MySelfLog.Contracts
{
    public class MySelfLogException : Exception
    {
        public MySelfLogException(string message) : base(message) { }

        public MySelfLogException(string message, Exception innerException) : base(message, innerException) { }
    }
}
