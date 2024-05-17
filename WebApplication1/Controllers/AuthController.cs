using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
        public ActionResult<User> Register(UserDTO request)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            user.Username = request.Username;
            user.PasswordHash = passwordHash;
            return Ok(user);
        }


        [HttpPost("Login")]
        public ActionResult<User> Login(UserDTO request)
        {
            try
            {
                if (user.Username != request.Username)
                {
                    return BadRequest("User not found on database");
                }


                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return BadRequest("Password wrong please reentry your password");
                }

                string token = CreateToken(user);
                return Ok(token);


            }
            catch (Exception ex)
            {
                return BadRequest(ex);
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

