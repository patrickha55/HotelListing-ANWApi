using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelListing.Data;
using HotelListing.Data.Configuration;
using HotelListing.Utilities.ServiceExts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Repository.IRepository;
using Repository.Repository;
using Services.AuthWithJWT;

namespace HotelListing.Webapi
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
            // Add db context
            services.AddDbContext<ApplicationContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("HotelHostingDB")));

            // Response Caching
            services.ConfigureHttpCacheHeaders();

            services.AddControllers().AddNewtonsoftJson(o => o.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            // Add cors
            services.ConfigureCors();

            
            // Automapper
            services.AddAutoMapper(typeof(MapperInitializer));
            
            // DI
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAuthManager, AuthManager>();
            
            // Add Identity
            services.AddAuthentication();
            services.ConfigureIdentity();
            services.ConfigureJwt(Configuration);

            // Add Api Versioning
            services.ConfigureVersioning();

            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "HotelListing.Webapi", Version = "v1"});
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HotelListing.Webapi v1"));
            }

            // Add Global Error Handling
            app.ConfigureExceptionHandler();

            app.UseHttpsRedirection();

            app.UseCors(ServiceExtensions.AllowAll);

            // Response Caching
            app.UseResponseCaching();
            app.UseHttpCacheHeaders();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}