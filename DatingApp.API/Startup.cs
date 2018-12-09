using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Http;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using DatingApp.API.Helpers;
using Newtonsoft.Json;
using AutoMapper;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DatingApp.API
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
             
            services.AddAutoMapper();
            services.RegisterServices();

            services.AddDbContext<DataContext>(x=>                     
                    x.UseMySql(GetConnectionString())
                    .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.IncludeIgnoredWarning)));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(opt=> {
                    opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; 
                });                
            
            services.AddCors();
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));
            services.AddTransient<Seed>();
            AddJwtConfiguration(services);
            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
            services.AddScoped<LogUserActivity>();
        }

        public void ConfigureDevelopmentServices(IServiceCollection services)
        {             
            services.AddAutoMapper();
            services.RegisterServices();
            services.AddDbContext<DataContext>(x=> x.UseMySql(GetConnectionString()));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(opt=> {
                    opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; 
                });
            services.AddCors();
            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));
            services.AddTransient<Seed>();
            AddJwtConfiguration(services);
            services.AddScoped<LogUserActivity>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, Seed seeder)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(builder => builder.Run(CustomMiddleware.ExceptionHandler));
            }

            // app.UseHttpsRedirection();
            //SeedUsers(app, seeder);

            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseAuthentication();

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc(routes => {
                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new {controller = "Fallback", action = "Index" }
                );
            });
        }   

        private string GetConnectionString()
        {
            return Configuration.GetConnectionString("DefaultConnection");
        }

        private void AddJwtConfiguration(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(Options =>
                {
                    Options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII
                            .GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });
        }

        private void SeedUsers(IApplicationBuilder app, Seed seeder)
        {
            //var seeder = (Seed)app.ApplicationServices.GetService(typeof(Seed));
            seeder.SeedUsers();
        }
    }
}
