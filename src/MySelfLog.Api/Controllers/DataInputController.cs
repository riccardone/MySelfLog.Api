﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        private const string IdField = "Id";
        private readonly IMultiTenantStore<MySelfLogTenantInfo> _store;
        private readonly IMessageSenderFactory _messageSenderFactory;

        /// <summary>
        /// Build controller
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="config"></param>
        /// <param name="store"></param>
        public DataInputController(ILogger<DataInputController> logger, IPayloadValidator payloadValidator, IIdGenerator idGenerator,
            IConfiguration config, IMultiTenantStore<MySelfLogTenantInfo> store,
                                   IMessageSenderFactory messageSenderFactory)
        {
            _logger = logger;
            _payloadValidator = payloadValidator;
            _idGenerator = idGenerator;
            _store = store;
            _messageSenderFactory = messageSenderFactory;
        }

        /// <summary>
        /// Build controller
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="store"></param>
        public DataInputController(ILogger<DataInputController> logger, IMultiTenantStore<MySelfLogTenantInfo> store, IMessageSenderFactory messageSenderFactory)
        {
            _logger = logger;
            _store = store;
            _messageSenderFactory = messageSenderFactory;
        }

        /// <summary>
        /// Retrieves an item using the id.
        /// </summary>
        /// <remarks>
        /// Optionally the item can be retrieved at a later date.
        /// </remarks>
        /// <response code="201">An insurance product has been successfully retrieved.</response>
        /// <response code="400">Unable to retrieve the item.</response>
        /// <response code="404">Unable to find an item with the given id.</response>
        /// <response code="408">The request to the server timed out. Please try again later.</response>
        /// <response code="500">Oops! Cannot retrieve the item at this time.</response>
        /// <param name="id"></param>
        [Route("{id}")]
        [HttpGet]
        [ProducesResponseType(typeof(JObject), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<JObject>> Get(string id)
        {
            throw new NotImplementedException();
        }

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
        public IActionResult Create([FromBody][Required] CloudEventRequest request)
        {
            if (!request.DataContentType.Equals("application/json"))
                return BadRequest("DataContentType must be: 'application/json'");

            if (string.IsNullOrWhiteSpace(request.Type))
                request.Type = "insuring";

            if (request.DataSchema != null && request.DataSchema.IsWellFormedOriginalString())
            {
                if (request.Data == null)
                {
                    return BadRequest("Data field is empty");
                }
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

                    return BadRequest(JsonConvert.SerializeObject(new { Error = errors.Replace("'", "") }, settings));
                }
                _logger.LogInformation($"Request received with valid schema (id:{request.Id};source:{request.Source};type:{request.Type};dataSchema:{request.DataSchema})");
            }
            else
                _logger.LogInformation($"Request received without schema (id:{request.Id};source:{request.Source};type:{request.Type})");

            JObject data = JsonConvert.DeserializeObject<JObject>(request.Data.ToString());
            var result = request.Id;

            //var hasIdentifier = false;
            //if (request.Type.Contains(""))
            //{
            //    foreach (KeyValuePair<string, JToken> property in data)
            //    {
            //        if (!property.Key.Equals(IdField)) continue;
            //        hasIdentifier = true;
            //        result = property.Value.ToString();
            //        break;
            //    }

            //    if (!hasIdentifier)
            //    {
            //        var id = new JValue(_idGenerator.GenerateId(string.Empty));
            //        data.Add(IdField, id);
            //        result = id.Value.ToString();
            //        request.Data = data;
            //    }
            //}

            EncryptMessageIfNeeded(request);
            var sender = _messageSenderFactory.Build(request.Source.ToString());
            sender.SendAsync(request).Wait();

            return Ok(new { id = result });
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
