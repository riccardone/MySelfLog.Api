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
using MySelfLog.Api.Extensions;
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

                var filePath = Path.Combine(AppContext.BaseDirectory, "Digital.Insuring.Api.xml");
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Chubb-API V1");
                c.SwaggerEndpoint("https://emeasit.chubbdigital.com/sales.chubbapi/swagger/v1/swagger.json", "PyS Chubb-API");
                c.RoutePrefix = string.Empty;
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "Digital Insuring API V2");
                c.SwaggerEndpoint("https://emeasit.chubbdigital.com/sales.chubbapi/swagger/v2/swagger.json", "Digital Insuring API");
                c.RoutePrefix = string.Empty;
            });
            app.ConfigureExceptionHandler(Logger);
            //app.UseMvc();
            app.UseRouting();
            app.UseCors("Open");
            app.UseMultiTenant();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute("default", "{__tenant__=}/{controller=}/{action=}");
            });
            app.UseHealthChecks("/health");
        }
    }
}
