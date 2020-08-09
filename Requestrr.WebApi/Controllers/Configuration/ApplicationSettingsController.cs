using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Requestrr.WebApi.config;

namespace Requestrr.WebApi.Controllers.Configuration
{
    [ApiController]
    [Authorize]
    [Route("/api/settings")]
    public class ApplicationSettingsController : ControllerBase
    {
        private readonly ApplicationSettings _applicationSettings;

        public ApplicationSettingsController(IOptionsSnapshot<ApplicationSettings> applicationSettingsAccessor)
        {
            _applicationSettings = applicationSettingsAccessor.Value;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            return Ok(new ApplicationSettingsModel
            {
                Port = _applicationSettings.Port,
            });
        }

        [HttpPost()]
        public async Task<IActionResult> SaveAsync([FromBody]ApplicationSettingsModel model)
        {
            _applicationSettings.Port = model.Port;

            ApplicationSettingsRepository.Update(_applicationSettings);

            return Ok(new { ok = true });
        }
    }
}