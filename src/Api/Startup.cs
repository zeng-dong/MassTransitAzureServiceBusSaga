﻿using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSwag;

namespace Api;

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
        services.AddControllers();
        services.AddSwaggerDocument(settings =>
        {
            settings.PostProcess = document =>
            {
                document.Info.Version = "v1";
                document.Info.Title = "Sample API";
                document.Info.Description = "A simple MassTransit API Sample";
                document.Info.TermsOfService = "None";
                document.Info.Contact = new OpenApiContact
                {
                    Name = "Some One",
                    Email = string.Empty,
                    Url = "https://github.com/someone"
                };
                document.Info.License = new OpenApiLicense
                {
                    Name = "Apache 2.0",
                    Url = "https://github.com/MassTransit/MassTransit/blob/develop/LICENSE"
                };
            };
        });

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.UsingAzureServiceBus((_, cfg) =>
            {
                cfg.Host(Configuration.GetConnectionString("AzureServiceBus"));
            });
        });
        services.AddMassTransitHostedService();

        services.AddGenericRequestClient();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseOpenApi();
            app.UseSwaggerUi3();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = HealthCheckResponseWriter
        });
        app.UseHealthChecks("/health/live", new HealthCheckOptions { ResponseWriter = HealthCheckResponseWriter });


        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

    static Task HealthCheckResponseWriter(HttpContext context, HealthReport result)
    {
        var json = new JObject(
            new JProperty("status", result.Status.ToString()),
            new JProperty("results", new JObject(result.Entries.Select(entry => new JProperty(entry.Key, new JObject(
                new JProperty("status", entry.Value.Status.ToString()),
                new JProperty("description", entry.Value.Description),
                new JProperty("data", JObject.FromObject(entry.Value.Data))))))));

        context.Response.ContentType = "application/json";

        return context.Response.WriteAsync(json.ToString(Formatting.Indented));
    }
}
