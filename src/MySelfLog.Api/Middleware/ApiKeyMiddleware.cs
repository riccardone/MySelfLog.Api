using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySelfLog.Admin.Model;
using MySelfLog.Contracts;

namespace MySelfLog.Api.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMultiTenantStore<MySelfLogTenantInfo> _store;
        public const string APIKEYNAME = "ApiKey";

        public ApiKeyMiddleware(RequestDelegate next, IMultiTenantStore<MySelfLogTenantInfo> store)
        {
            _next = next;
            _store = store;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Api Key was not provided");
                return;
            }

            string jsonString;
            context.Request.EnableBuffering();
            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, false, 1024, true))
            {
                jsonString = await reader.ReadToEndAsync();
                context.Request.Body.Seek(0, SeekOrigin.Begin);
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            var cloudEvent = JsonSerializer.Deserialize<CloudEventRequest>(jsonString, options);
            if (cloudEvent == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("invalid request");
                return;
            }
            var tenant = await _store.TryGetByIdentifierAsync(cloudEvent.Source.ToString());
            if (tenant == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("tenant not found");
                return;
            }

            var apiKey = tenant.ApiKey;

            if (!apiKey.Equals(extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized client");
                return;
            }

            await _next(context);
        }
    }
}
