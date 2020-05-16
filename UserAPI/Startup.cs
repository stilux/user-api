using System.Net.Mime;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Prometheus;
using Swashbuckle.AspNetCore.Swagger;
using UserAPI.Filters;
using UserAPI.Providers;

namespace UserAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options =>
                {
                    options.Filters.Add<ValidationFilter>();
                    options.Filters.Add<HttpResponseExceptionFilter>();
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var result = new BadRequestObjectResult(context.ModelState);
                        result.ContentTypes.Add(MediaTypeNames.Application.Json);
                        return result;
                    };
                })
                .AddFluentValidation(fv =>
                {
                    fv.RegisterValidatorsFromAssemblyContaining<Startup>();
                });

            services.AddEntityFrameworkNpgsql()
                .AddDbContext<UserDbContext>(options
                    => options.UseNpgsql(_configuration.GetConnectionString("UserDbConnection")));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = $"{_environment.ApplicationName}", Version = "v1"});
                c.AddFluentValidationRules();
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseHttpMetrics();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{_environment.ApplicationName}  V1");
                c.RoutePrefix = string.Empty;
            });
        }
    }
}