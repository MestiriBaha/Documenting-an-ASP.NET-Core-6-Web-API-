using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Library.API.Authentication
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> optionsMonitor,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(optionsMonitor,logger,encoder,clock)
        {

        }
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if(!Request.Headers.ContainsKey("Authentication"))
            {
                return Task.FromResult(AuthenticateResult.Fail("Authentication failed")); 
            }
            try
            {
                var authenticationheader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authenticationheader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
                var username = credentials[0];
                var password = credentials[1];

                if (username == "Pluralsight" && password == "Pluralsight")
                {
                    var claims = new[] {
                        new Claim(ClaimTypes.NameIdentifier, username)};
                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);

                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }
                return Task.FromResult(
                 AuthenticateResult.Fail("Invalid username or password"));
            

                    } 
            catch
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header")); 
            }

        }
    }
}
