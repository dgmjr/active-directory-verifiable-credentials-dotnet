using System;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel;
using Microsoft.IdentityModel.Logging;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.IdentityModel.Tokens.Jwt; // Add this using directive

namespace AspNetCoreVerifiableCredentials
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
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
            IdentityModelEventSource.ShowPII = true;
            //configure AAD signin. When issuing VCs the users need to sign in first.
            services
                .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(
                    Configuration.GetSection("AzureAD"),
                    subscribeToOpenIdConnectMiddlewareDiagnosticsEvents: true,
                    displayName: "Azure Active Driectory"
                );
            //TODO check if this is needed, I only want the issuing website to authenticate the user
            services.AddAuthorization(options =>
            {
                //  By default, all incoming requests will be authorized according to the default policy
                options.FallbackPolicy = options.DefaultPolicy;
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.Configure<AppSettingsModel>(Configuration.GetSection("AppSettings"));

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(1); //You can set Time
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // services
            // .AddSwaggerGen(opts => { });

            services.AddRazorPages().AddMvcOptions(options => { }).AddMicrosoftIdentityUI();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            IdentityModelEventSource.ShowPII = true;
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseSession();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            //we want the user to be able to sign-in when VCs are being issued
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI(opts =>
            {
                opts.EnableTryItOutByDefault();
                opts.DefaultModelExpandDepth(0);
                opts.DefaultModelRendering(ModelRendering.Model);
                opts.DisplayOperationId();
                opts.EnableFilter();
                opts.DocExpansion(DocExpansion.None);
                opts.EnablePersistAuthorization();
                opts.DisplayRequestDuration();
                opts.ShowCommonExtensions();
                opts.ShowExtensions();
                opts.SupportedSubmitMethods(
                    SubmitMethod.Get,
                    SubmitMethod.Head,
                    SubmitMethod.Post,
                    SubmitMethod.Put,
                    SubmitMethod.Trace,
                    SubmitMethod.Delete,
                    SubmitMethod.Options,
                    SubmitMethod.Options
                );
            });

            app.UseCookiePolicy(new CookiePolicyOptions { Secure = CookieSecurePolicy.Always });

            //this setting is used when you use tools like ngrok or reverse proxies like nginx which connect to http://localhost
            //if you don't set this setting the sign-in redirect will be http instead of https
            app.UseForwardedHeaders(
                new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedProto }
            );

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
