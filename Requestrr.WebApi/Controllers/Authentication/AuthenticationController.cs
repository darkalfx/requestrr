using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Requestrr.WebApi.Config;

namespace Requestrr.WebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("/api/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthenticationSettings _authenticationSettings;

        public AuthenticationController(IOptionsSnapshot<AuthenticationSettings> authenticationSettingsAccessor)
        {
            _authenticationSettings = authenticationSettingsAccessor.Value;
        }

        [HttpGet("validate")]
        public async Task<IActionResult> Validate()
        {
            return Ok(new { ok = true});
        }

        [AllowAnonymous]
        [HttpGet("registration")]
        public async Task<IActionResult> IsRegistered()
        {
            return Ok(new
            {
                hasRegistered = !string.IsNullOrWhiteSpace(_authenticationSettings.Username) && !string.IsNullOrWhiteSpace(_authenticationSettings.Password)
            });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody]LoginModel model)
        {
            var logonSuccessful = _authenticationSettings.Username.Equals(model.Username, StringComparison.InvariantCultureIgnoreCase) && _authenticationSettings.Password == EncryptPassword(model.Password);

            if (!logonSuccessful)
                return Unauthorized("Username or password is incorrect.");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationSettings.PrivateKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, model.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
              issuer: "Requestrr",
              audience: "Requestrr",
              claims,
              expires: DateTime.Now.AddMonths(1),
              signingCredentials: credentials
            );

            var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { token = encodedToken });
        }

        [HttpPost("password")]
        public async Task<IActionResult> ChangePasswordAsync([FromBody]ChangePasswordModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ExistingPassword)
                || string.IsNullOrWhiteSpace(model.NewPassword)
                || string.IsNullOrWhiteSpace(model.NewPasswordConfirmation)
                || !model.NewPassword.Equals(model.NewPasswordConfirmation, StringComparison.InvariantCultureIgnoreCase))
            {
                return BadRequest("Password confirmation and password do not match.");
            }

            if (!_authenticationSettings.Password.Equals(EncryptPassword(model.ExistingPassword), StringComparison.InvariantCultureIgnoreCase))
            {
                return BadRequest("The password provided was incorrect.");
            }

            AuthenticationSettingsRepository.UpdateAdminAccount(_authenticationSettings.Username, EncryptPassword(model.NewPassword));

            return Ok(new { ok = true });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody]RegistrationModel model)
        {
            if (!string.IsNullOrWhiteSpace(_authenticationSettings.Username) || !string.IsNullOrWhiteSpace(_authenticationSettings.Password))
            {
                return Conflict("Admin account has already been created.");
            }

            if (string.IsNullOrWhiteSpace(model.Username)
                || string.IsNullOrWhiteSpace(model.Password)
                || string.IsNullOrWhiteSpace(model.PasswordConfirmation)
                || !model.Password.Equals(model.PasswordConfirmation, StringComparison.InvariantCultureIgnoreCase))
            {
                return BadRequest("Registration information was incorrect.");
            }

            AuthenticationSettingsRepository.UpdateAdminAccount(model.Username, EncryptPassword(model.Password));

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationSettings.PrivateKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, model.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
              issuer: "Requestrr",
              audience: "Requestrr",
              claims,
              expires: DateTime.Now.AddMonths(1),
              signingCredentials: credentials
            );

            var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { token = encodedToken });
        }

        private string EncryptPassword(string password)
        {
            var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: Encoding.UTF8.GetBytes(_authenticationSettings.PrivateKey),
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

            return hashed;
        }
    }
}
