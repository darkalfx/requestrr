using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using System.Security.Claims;
using System.Collections.Generic;

namespace Requestrr.WebApi
{
    public class DisabledAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public DisabledAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(), "Disabled"));
            var ticket = new AuthenticationTicket(principal, "Disabled");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}