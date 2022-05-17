using DAL.Data;
using DAL.Data.Entities;
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
    //[Authorize]
    public class AuthController : ControllerBase
    {
        private Context _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthController(Context context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginModel model)
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


                        // Add your claims to the JWT token
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
                    return Ok("No user or password matched, try again.");
                }
            }
            else
            {
                return Ok("No such user exists");
            }

        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterModel model)
        {
            // Always better with a global try catch
            User newUser = new User()
            {
                Email = model.Email,
                UserName = model.Username,
                //AgreedToTerms = model.AgreedToTerms,
                EmailConfirmed = false,
                Id = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(newUser, model.Password);
            if (!result.Succeeded)
            {
                var exceptionText = result.Errors.Aggregate("User Creation Failed - Identity Exception. Errors were: \n\r\n\r", (current, error) => current + (" - " + error + "\n\r"));
                throw new Exception(exceptionText);
            }
            if (result.Succeeded)
            {
                User user = await _userManager.FindByNameAsync(newUser.UserName);
                if (user is not null)
                {
                    await _userManager.AddToRoleAsync(newUser, "User");

                    //Remember to set your custom data and relationships here

                    //UserSettings settings = new UserSettings()
                    //{
                    //    Id = Guid.NewGuid().ToString(),
                    //    DarkMode = true,
                    //    User = user
                    //};

                    //UserGDPR gdpr = new UserGDPR()
                    //{
                    //    Id = Guid.NewGuid().ToString(),
                    //    UseMyData = false,
                    //    User = user
                    //};

                    ////Add it to the context
                    //_context.UserSettings.Add(settings);
                    //_context.UserGDPR.Add(gdpr);

                    //Save the data
                    _context.SaveChanges();

                    return Ok(new { result = $"User {model.Username} has been created", Token = "xxx" });
                }
                else
                {
                    return Ok(new { message = "Registration failed for unknown reasons, please try again." });
                }
            }
            else
            {
                StringBuilder errorString = new StringBuilder();

                foreach (var error in result.Errors)
                {
                    errorString.Append(error.Description);
                }

                return Ok(new { result = $"Register Fail: {errorString.ToString()}" });
            }

        }
    }
}
