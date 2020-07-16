using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace adfs_net_core
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
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
            services.AddRazorPages();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "cookie";
                options.DefaultChallengeScheme = "oidc";
            })
                .AddCookie("cookie", options =>
                {
                    options.Cookie.Name = "mycookie";

                    options.Events.OnSigningOut = async e =>
                    {
                        await e.HttpContext.RevokeUserRefreshTokenAsync();
                    };
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = Configuration["ADFS:Authority"];

                    options.ClientId = Configuration["ADFS:ClientId"];
                    options.ClientSecret = Configuration["ADFS:ClientSecret"];

                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.ResponseMode = OpenIdConnectResponseMode.FormPost;
                    options.Scope.Add("openid");
                    options.Scope.Add("email");
                    options.Scope.Add("profile");
                    options.Scope.Add("allatclaims");
                    options.Scope.Add("offline_access");
                    options.CallbackPath = new PathString("/callback");
                    options.SaveTokens = true;

                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.Use(next => context =>
            {
                if (string.Equals(context.Request.Headers["X-Forwarded-Proto"], "https", StringComparison.OrdinalIgnoreCase))
                {
                    context.Request.Scheme = "https";
                }

                return next(context);
            });

            app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });

            IdentityModelEventSource.ShowPII = true;

        }
    }
}
