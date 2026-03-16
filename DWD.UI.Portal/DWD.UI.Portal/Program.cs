using DWD.UI.Portal.Client.Pages;
using DWD.UI.Portal.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Okta.AspNetCore;

namespace DWD.UI.Portal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

            builder.Services.AddControllers();

            // Configure authentication (OpenID Connect) for Blazor Server
            // Settings are read from configuration (appsettings.json / environment).
            // Ensure you register the exact callback URLs in Okta:
            var oktaPostLogoutUri = builder.Configuration["Okta:PostLogoutRedirectUri"] ?? "https://localhost:44314";
            var oktaRedirectUri = builder.Configuration["Okta:LoginRedirectUri"] ?? "https://localhost:44314/authorization-code/callback";
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddOktaMvc(new OktaMvcOptions
            {
                // Replace the Okta placeholders in appsettings.json with your Okta configuration.
                OktaDomain = builder.Configuration.GetValue<string>("Okta:OktaDomain"),
                ClientId = builder.Configuration.GetValue<string>("Okta:ClientId"),
                ClientSecret = builder.Configuration.GetValue<string>("Okta:ClientSecret"),
            });
            // Configure OpenIdConnect events to log (and optionally override) redirect_uri
            builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Events ??= new OpenIdConnectEvents();
                options.Events.OnRedirectToIdentityProvider = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                        .CreateLogger("OpenIdConnect");
                    logger.LogInformation("Outgoing OIDC redirect_uri: {RedirectUri}", context.ProtocolMessage.RedirectUri);

                    // Example: override to a known registered URI (uncomment to force)
                    context.ProtocolMessage.RedirectUri = oktaRedirectUri;

                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToIdentityProviderForSignOut = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                        .CreateLogger("OpenIdConnect");
                    logger.LogInformation("Outgoing OIDC post_logout_redirect_uri: {PostLogoutRedirectUri}",
                        context.ProtocolMessage.PostLogoutRedirectUri);
                    // Example: override to a known registered URI (uncomment to force)
                    context.ProtocolMessage.PostLogoutRedirectUri = oktaPostLogoutUri;
                    return Task.CompletedTask;
                };
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            app.Run();
        }
    }
}
