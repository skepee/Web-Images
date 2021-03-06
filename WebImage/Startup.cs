﻿using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using System;
using Microsoft.AspNetCore.Identity.UI.Services;
using WebImage.DBContext;
using WebImage.Models;

namespace WebImage
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
            Settings.ConnectionString = Configuration.GetConnectionString("DefaultConnection");
            Settings.ClientId = Configuration.GetSection("Azure").GetChildren().ElementAt(0).Value;
            Settings.ClientSecret = Configuration.GetSection("Azure").GetChildren().ElementAt(1).Value;
            Settings.Secrets = Configuration.GetSection("Azure").GetChildren().ElementAt(2).Value;
            Settings.StorageAccount = Configuration.GetSection("Azure").GetChildren().ElementAt(3).Value;
            Settings.TenantId = Configuration.GetSection("Azure").GetChildren().ElementAt(4).Value;

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddHttpContextAccessor();
            services.AddRazorPages();
            services.AddMemoryCache();
            services.AddEntityFrameworkSqlServer();
            services.AddDbContext<IjpContext>(options => options.UseSqlServer(Settings.ConnectionString));
            services.AddDbContext<IjpDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<IjpDbContext>();
            services.AddMvc(option=>option.EnableEndpointRouting=false);

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters ="abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            // using Microsoft.AspNetCore.Identity.UI.Services;
            //TODO
            //services.AddSingleton<IEmailSender, EmailSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //string cachePeriod = env.IsDevelopment() ? "600" : "604800";

            //var urlImage = Path.Combine(env.WebRootPath, "imagefolder");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseRouting();
            app.UseAuthorization();
            app.UseStaticFiles();
            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    FileProvider = new PhysicalFileProvider(urlImage),
            //    RequestPath = "/Images",

            //    OnPrepareResponse = ctx =>
            //    {
            //        ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cachePeriod}");
            //    },
            //});

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), @"node_modules")),
                RequestPath = new PathString("/vendor")
            });

            app.UseHttpsRedirection();
            app.UseCookiePolicy();
            //app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                //endpoints.MapControllerRoute(
                //    name: "default",
                //    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseMvcWithDefaultRoute();
        }
    }
}
