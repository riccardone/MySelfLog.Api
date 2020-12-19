using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Logging;
using MySelfLog.Admin.Model;
using MySelfLog.Api.Middleware;
using MySelfLog.Api.Services;
using MySelfLog.Contracts.Api;
using NLog;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MySelfLog.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

        public IConfiguration Configuration { get; }
        public ILogger<Startup> Logger { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApiVersioning(o =>
            {
                o.ApiVersionReader = new HeaderApiVersionReader("apiVersion");
                //o.ApiVersionSelector = new ConstantApiVersionSelector(new ApiVersion(1, 0));
                o.AssumeDefaultVersionWhenUnspecified = true;
            });

            services.AddScoped<IIdGenerator, IdGenerator>();
            services.AddScoped<IResourceLocator, FileLocator>();
            services.AddScoped<ISchemaProvider, SchemaProvider>();
            services.AddScoped<IResourceElements, ResourceElements>();
            services.AddScoped<IPayloadValidator, PayloadValidator>();
            services.AddScoped<IMessageSenderFactory, MessageSenderFactory>();
            services.AddScoped<IBusSettings, BusSettings>();

            services.AddControllers()
                .AddControllersAsServices()
                .AddNewtonsoftJson();

            services.AddMultiTenant<MySelfLogTenantInfo>()
                .WithConfigurationStore();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MySelfLog-Api", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityDefinition(ApiKeyMiddleware.APIKEYNAME, new OpenApiSecurityScheme
                {
                    Description = "Api key needed to access the endpoints. X-Api-Key: My_API_Key",
                    In = ParameterLocation.Header,
                    Name = ApiKeyMiddleware.APIKEYNAME,
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Name = ApiKeyMiddleware.APIKEYNAME,
                            Type = SecuritySchemeType.ApiKey,
                            In = ParameterLocation.Header,
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = ApiKeyMiddleware.APIKEYNAME
                            },
                        },
                        new string[] {}
                    }
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        }, new List<string>() }
                });

                c.OperationFilter<ApiVersionHeaderParameterOperationFilter>();
                c.OperationFilter<AuthorizationHeaderParameterOperationFilter>();

                //Use method name as operationId
                c.CustomOperationIds(apiDesc => apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null);

                var filePath = Path.Combine(AppContext.BaseDirectory, "MySelfLog.Api.xml");
                c.IncludeXmlComments(filePath);
            });
            services.AddHealthChecks();

            services.AddCors(options =>
            {
                options.AddPolicy(
                    "Open",
                    builder => builder.AllowAnyOrigin().AllowAnyHeader());
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAuthentication();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "MySelfLog API V1");
                c.RoutePrefix = string.Empty;
            });
            app.ConfigureExceptionHandler(Logger);
            //app.UseMvc();
            app.UseRouting();
            app.UseAuthorization();
            app.UseMiddleware<ApiKeyMiddleware>();
            app.UseCors("Open");
            app.UseMultiTenant();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute("default", "{__tenant__=}/{controller=}/{action=}");
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
