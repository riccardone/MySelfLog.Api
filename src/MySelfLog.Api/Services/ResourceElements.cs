using System;
using System.Text.RegularExpressions;

namespace MySelfLog.Api.Services
{
    public class ResourceElements : IResourceElements
    {
        /// <summary>
        /// Given a uri, returns the client, version and filename
        /// </summary>
        /// <param name="uriLcase"></param>
        /// <returns></returns>
        public Tuple<string, string> GetResourceElements(string uri)
        {
            try
            {
                if (String.IsNullOrEmpty(uri))
                    throw new NullReferenceException("Schema Url cannot be empty");

                string uriLCase = uri.ToLower();

                if (uriLCase.EndsWith(".json"))
                    uriLCase = Regex.Replace(uriLCase, "\\/[A-Za-z0-9]*\\.json", "");


                //Check if there is a trailing slash and remove it
                if (uriLCase[uriLCase.Length-1].Equals("/"))
                    uriLCase = uriLCase.Substring(0, uriLCase.Length - 1);

                var uriElements = uriLCase.Split("/");

                var version = uriElements[uriElements.Length - 1];
                var client = uriElements[uriElements.Length - 2];

                return new Tuple<string, string>(client, version);
            }
            catch (IndexOutOfRangeException)
            {
                throw new Exception(
                    "The schema must be in the form <domain>/<version>. Example: acme/1.0");
            }
        }
    }
}
