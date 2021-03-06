using DAL.Data;
using DAL.Data.Entities;
using DAL.Services;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NewsAccessAPI.Data;
using NewsAccessAPI.Data.Entities;
using NewsAccessAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Context = NewsAccessAPI.Data.Context;

namespace NewsAccessAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private Context _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private ILog _logger;
        public AuthController(Context context, UserManager<User> userManager, SignInManager<User> signInManager, ILogger logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger.GetLogger(typeof(AuthController));
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                User user = model.User.Contains("@") ? await _userManager.FindByEmailAsync(model.User) : await _userManager.FindByNameAsync(model.User);
                if (user != null)
                {
                    var signInResult = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

                    if (signInResult.Succeeded)
                    {
                        Config config = new Config();
                        using (StreamReader r = new StreamReader("Config.json"))
                        {
                            string json = r.ReadToEnd();
                            config = JsonConvert.DeserializeObject<Config>(json);
                        }
                        var roles = await _userManager.GetRolesAsync(user);
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var key = Encoding.ASCII.GetBytes(config.Key);
                        var tokenDescriptor = new SecurityTokenDescriptor
                        {
                            Subject = new ClaimsIdentity(new Claim[]
                            {
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(ClaimTypes.Role, roles.First().ToString())

                            }),
                            Expires = DateTime.UtcNow.AddDays(1),
                            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                        };

                        var token = tokenHandler.CreateToken(tokenDescriptor);
                        var tokenString = tokenHandler.WriteToken(token);

                        return Ok(new { Token = tokenString });
                    }
                    else
                    {
                        return BadRequest("No user or password matched, try again.");
                    }
                }
                else
                {
                    return BadRequest("No such user exists");
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return StatusCode(500);
            }
            

        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                User newUser = new User()
                {
                    Email = model.Email,
                    UserName = model.Username,
                    EmailConfirmed = false,
                    Id = Guid.NewGuid().ToString()
                };

                var result = await _userManager.CreateAsync(newUser, model.Password);
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors.FirstOrDefault().Description);
                }
                if (result.Succeeded)
                {
                    User user = await _userManager.FindByNameAsync(newUser.UserName);
                    if (user is not null)
                    {
                        await _userManager.AddToRoleAsync(newUser, "User");
                        
                        _context.SaveChanges();

                        return Ok(new { result = $"User {model.Username} has been created" });
                    }
                    else
                    {
                        return BadRequest(new { message = "Registration failed for unknown reasons, please try again." });
                    }
                }
                else
                {
                    StringBuilder errorString = new StringBuilder();

                    foreach (var error in result.Errors)
                    {
                        errorString.Append(error.Description);
                    }

                    return BadRequest(new { result = $"Register Fail: {errorString.ToString()}" });
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return StatusCode(500);
            }
            

        }
    }
}
