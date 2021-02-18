using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Crypto;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySelfLog.Admin.Model;
using MySelfLog.Api.Services;
using MySelfLog.Contracts;
using MySelfLog.Contracts.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MySelfLog.Api.Controllers
{
    /// <summary>
    /// DataInput controller
    /// </summary>
    [Route("/[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = false)]
    public class DataInputController : ControllerBase
    {
        private readonly ILogger<DataInputController> _logger;
        private readonly IPayloadValidator _payloadValidator;
        private readonly IIdGenerator _idGenerator;
        private const string IdField = "CorrelationId";
        private readonly IMultiTenantStore<MySelfLogTenantInfo> _store;
        private readonly IMessageSenderFactory _messageSenderFactory;

        /// <summary>
        /// Build controller
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="idGenerator"></param>
        /// <param name="store"></param>
        /// <param name="messageSenderFactory"></param>
        /// <param name="payloadValidator"></param>
        public DataInputController(ILogger<DataInputController> logger, IPayloadValidator payloadValidator,
            IIdGenerator idGenerator, IMultiTenantStore<MySelfLogTenantInfo> store,
            IMessageSenderFactory messageSenderFactory)
        {
            _logger = logger;
            _payloadValidator = payloadValidator;
            _idGenerator = idGenerator;
            _store = store;
            _messageSenderFactory = messageSenderFactory;
        }

        ///// <summary>
        ///// Retrieves an item using the id.
        ///// </summary>
        ///// <remarks>
        ///// Optionally the item can be retrieved at a later date.
        ///// </remarks>
        ///// <response code="201">An insurance product has been successfully retrieved.</response>
        ///// <response code="400">Unable to retrieve the item.</response>
        ///// <response code="404">Unable to find an item with the given id.</response>
        ///// <response code="408">The request to the server timed out. Please try again later.</response>
        ///// <response code="500">Oops! Cannot retrieve the item at this time.</response>
        ///// <param name="id"></param>
        //[Route("{id}")]
        //[HttpGet]
        //[ProducesResponseType(typeof(JObject), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //public async Task<ActionResult<JObject>> Get(string id)
        //{
        //    return  NotFound();
        //}

        /// <summary>
        /// Ingest message synchronously
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="201">The message has been successfully ingested</response>
        /// <response code="404">Unable to ingest the message</response>
        /// <param name="request"></param>
        [HttpPost]
        [Consumes("application/cloudevents+json")]
        [ProducesResponseType(typeof(CloudEventRequest), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody][Required] CloudEventRequest request)
        {
            _logger.LogInformation("DataInput->Create is called");

            if (!request.DataContentType.Equals("application/json"))
                return BadRequest("DataContentType must be: 'application/json'");

            if (string.IsNullOrWhiteSpace(request.Type))
                return BadRequest("Type must be set");

            if (!IsValid(request, out var badRequest)) 
                return badRequest;

            var data = JsonConvert.DeserializeObject<JObject>(request.Data.ToString());

            var result = CheckIfMessageNeedCorrelationId(request, data, request.Id);

            EncryptMessageIfNeeded(request);

            var sender = _messageSenderFactory.Build(request.Source.ToString());
            await sender.SendAsync(request);

            return Ok(new { CorrelationId = result });
        }

        private bool IsValid(CloudEventRequest request, out IActionResult badRequest)
        {
            badRequest = null;
            if (request.DataSchema != null && request.DataSchema.IsWellFormedOriginalString())
            {
                var validationResult = _payloadValidator.Validate(request.DataSchema.ToString(), request.Data.ToString());

                if (!validationResult.IsValid)
                {
                    string errors = string.Join(',', validationResult.ErrorMessages);
                    _logger.LogWarning(
                        $"Request received with invalid schema (id:{request.Id};source:{request.Source};type:{request.Type};dataSchema:{request.DataSchema};errors:{errors})");

                    var settings = new JsonSerializerSettings
                    {
                        StringEscapeHandling = StringEscapeHandling.EscapeHtml
                    };

                    badRequest = BadRequest(JsonConvert.SerializeObject(new {Error = errors.Replace("'", "")}, settings));
                    return false;
                }

                _logger.LogInformation(
                    $"Request received with valid schema (id:{request.Id};source:{request.Source};type:{request.Type};dataSchema:{request.DataSchema})");
            }
            else
                _logger.LogInformation(
                    $"Request received without schema (id:{request.Id};source:{request.Source};type:{request.Type})");

            return true;
        }

        private string CheckIfMessageNeedCorrelationId(CloudEventRequest request, JObject data, string result)
        {
            var hasIdentifier = false;
            foreach (KeyValuePair<string, JToken> property in data)
            {
                if (!property.Key.ToLower().Equals(IdField.ToLower())) continue;
                hasIdentifier = true;
                result = property.Value.ToString();
                break;
            }

            if (!hasIdentifier)
            {
                var id = new JValue(_idGenerator.GenerateId(string.Empty));
                data.Add(IdField, id);
                result = id.Value.ToString();
                request.Data = data;
            }

            return result;
        }

        private void EncryptMessageIfNeeded(CloudEventRequest request)
        {
            var tenant = _store.TryGetByIdentifierAsync(request.Source.ToString()).Result;
            if (tenant == null || string.IsNullOrWhiteSpace(tenant.CryptoKey))
                return;
            var cryptoService = new AesCryptoService(Convert.FromBase64String(tenant.CryptoKey));
            request.Data = Convert.ToBase64String(cryptoService.Encrypt(JsonConvert.SerializeObject(request.Data)));
            // Example to decrypt
            //var test = cryptoService.Decrypt( Convert.FromBase64String(request.Data));
        }
    }
}
