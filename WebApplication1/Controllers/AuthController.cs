using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        public static User user = new User();
        public readonly IConfiguration _configuration;
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }



        [HttpPost("register")]
        [Authorize]
        public ActionResult<User> Register(UserDTO request)
        {
            var passwordHasher = new PasswordHasher<UserDTO>();
            string passwordHash = passwordHasher.HashPassword(request, request.Password);

            var user = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash
            };

            return Ok(user);
        }



        [HttpPost("login")]
        public ActionResult<string> Login(User request)
        {
            try
            {
                string token = CreateToken(request);
                return Ok(token);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>();
            {
                new Claim(ClaimTypes.Name, user.Username);
            }


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
               expires: DateTime.Now.AddDays(1),
               signingCredentials: credentials
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;


        }

    }
}

