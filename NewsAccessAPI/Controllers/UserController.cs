using DAL.Services;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace NewsAccessAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private ILog _logger;

        public UserController(ILogger logger)
        {
            _logger = logger.GetLogger(typeof(UserController));
        }
        [Authorize]
        [HttpPost("updatesettings")]
        public async Task<ActionResult> UpateUserSettings()
        {
            return Ok();
        }
    }
}
