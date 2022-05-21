using DAL;
using DAL.Data;
using DAL.Data.Entities;
using DAL.Services;
using log4net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsAccessAPI.Data;
using NewsAccessAPI.Data.Entities;
using NewsAccessAPI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NewsAccessAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    //[Authorize]
    public class UserController : ControllerBase
    {
        private ILog _logger;
        private UserManager<User> _userManager;
        private Context _context;

        public UserController(ILogger logger, UserManager<User> userManager, Context context)
        {
            _logger = logger.GetLogger(typeof(UserController));
            _userManager = userManager;
            _context = context;
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("getsettings")]
        public async Task<ActionResult> GetUserSettings()
        {
            var user = await _userManager.FindByNameAsync(User.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.Name)).Value);

            var countries = _context.Countries.Where(x => x.UserId == user.Id).ToList().Select(x => x.CountryName);
            var categoris = _context.categoris.Where(x => x.UserId == user.Id).ToList().Select(x => x.CategoriName);
            var sourceBlackList = _context.sourceBlackList.Where(x => x.UserId == user.Id).ToList().Select(x => x.SourceName);

            return Ok(new { countries  = countries, categoris = categoris, sourceBlackList = sourceBlackList });
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("updatesettings")]
        public async Task<ActionResult> UpdateUserSettings([FromBody] UserSettingsModel model)
        {
            var user = await _userManager.FindByNameAsync(User.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.Name)).Value);

            var test = _context.Users.Include(x => x.Categoris).Include(x => x.Countries).Include(x => x.SourceBlackList).Where(x => x.Id == user.Id).FirstOrDefault();

            foreach (var categori in user.Categoris)
            {
                _context.categoris.Remove(categori);
            }

            foreach (var categori in model.Categoris)
            {
                user.Categoris.Add(new Data.Entities.Categori { Id = Guid.NewGuid().ToString(), CategoriName = categori });
            }

            foreach (var country in user.Countries)
            {
                _context.Countries.Remove(country);
            }

            foreach (var country in model.Countries)
            {
                user.Countries.Add(new Country { Id = Guid.NewGuid().ToString(), CountryName = country });

            }

            foreach (var sourceBlackListItem in user.SourceBlackList)
            {
                _context.sourceBlackList.Remove(sourceBlackListItem);
            }

            foreach (var SourceBlackListItem in model.SourceBlackList)
            {
                user.SourceBlackList.Add(new SourceBlackListItem { Id = Guid.NewGuid().ToString(), SourceName = SourceBlackListItem });

            }

            _context.SaveChanges();

            return Ok();
        }
    }
}


