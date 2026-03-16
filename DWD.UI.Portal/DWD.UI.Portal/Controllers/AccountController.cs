using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DWD.UI.Portal.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        // GET /account/signin?returnUrl=/some/path
        [HttpGet("signin")]
        [AllowAnonymous]
        public IActionResult SignIn([FromQuery] string? returnUrl)
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return LocalRedirect(returnUrl ?? Url.Content("~/"));
            }

            var props = new AuthenticationProperties
            {
                RedirectUri = returnUrl ?? Url.Content("~/")
            };

            // Challenge the OpenID Connect handler
            return Challenge(props, OpenIdConnectDefaults.AuthenticationScheme);
        }

        // GET /account/signout?returnUrl=/
        [HttpGet("signout")]
        public IActionResult SignOut([FromQuery] string? returnUrl)
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return LocalRedirect(returnUrl ?? Url.Content("~/"));
            }

            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Content(returnUrl ?? "~/")
            };

            // Sign out at the IdP and locally (cookie)
            return SignOut(props,
                OpenIdConnectDefaults.AuthenticationScheme,
                CookieAuthenticationDefaults.AuthenticationScheme);
        }

        //// This endpoint receives the IdP post-logout redirect.
        //// Ensure this exact path is registered as the PostLogoutRedirectUri in Okta:
        //// http://localhost:8080/signout-callback-oidc
        //[HttpGet("signout-callback-oidc")]
        //[AllowAnonymous]
        //public IActionResult SignOutCallback()
        //{
        //    // If the user is still authenticated locally, redirect home (middleware may also clear cookie)
        //    if (User?.Identity?.IsAuthenticated == true)
        //    {
        //        return LocalRedirect(Url.Content("~/"));
        //    }

        //    // Landing page after remote logout - redirect to home or show a signed out page
        //    return Redirect(Url.Content("~/"));
        //}

        //// Optional: access denied route
        //// GET /account/access-denied
        //[HttpGet("access-denied")]
        //[AllowAnonymous]
        //public IActionResult AccessDenied()
        //{
        //    return Forbid();
        //}
    }
}
