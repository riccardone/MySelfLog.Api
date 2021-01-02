using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MySelfLog.Admin.Model;
using MySelfLog.Api.Services;
using MySelfLog.Contracts.Api;

namespace MySelfLog.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IIdGenerator, IdGenerator>();
            services.AddScoped<IResourceLocator, FileLocator>();
            services.AddScoped<ISchemaProvider, SchemaProvider>();
            services.AddScoped<IResourceElements, ResourceElements>();
            services.AddScoped<IPayloadValidator, PayloadValidator>();
            services.AddScoped<IMessageSenderFactory, MessageSenderFactory>();
            services.AddControllers();
            services.AddMultiTenant<MySelfLogTenantInfo>()
                .WithConfigurationStore();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MySelfLog.Api", Version = "v1" });
            });
            services.AddHealthChecks();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MySelfLog.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseHealthChecks("/health");
        }
    }
}
