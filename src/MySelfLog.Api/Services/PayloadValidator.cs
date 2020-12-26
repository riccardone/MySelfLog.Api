using System;
using System.Collections.Generic;
using MySelfLog.Contracts.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace MySelfLog.Api.Services
{
    public class PayloadValidator : IPayloadValidator
    {
        private readonly IResourceElements _resourceElements;
        private readonly ISchemaProvider _schemaProvider;

        public PayloadValidator(IResourceElements resourceElements, ISchemaProvider schemaProvider)
        {
            _resourceElements = resourceElements;
            _schemaProvider = schemaProvider;
        }

        public PayloadValidationResult Validate(string schemaUri, object value)
        {
            try
            {
                if (value is null)
                    throw new Exception("the value to be validated can't be null");

                var jsonValue = JObject.Parse(value.ToString());

                var resourceElements =_resourceElements.GetResourceElements(schemaUri);

                var schema = _schemaProvider.GetSchema(resourceElements.Item1, resourceElements.Item2);

                //Create Json Schema object
                var jsonSchema = JSchema.Parse(schema);
                jsonSchema.AllowAdditionalProperties = false;

                var isValid = jsonValue.IsValid(jsonSchema, out IList<string> errorMessages);

                return new PayloadValidationResult(isValid, errorMessages);
            }
            catch (JsonReaderException readerException)
            {
                return new PayloadValidationResult(false, $"Error while validating input value (value is expected to be json): {readerException.Message}");
            }
            catch (Exception exception)
            {
                return new PayloadValidationResult(false, exception.Message);
            }
        }
    }
}
