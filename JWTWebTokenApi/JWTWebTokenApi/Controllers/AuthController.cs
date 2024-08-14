using BCrypt.Net;
using JWTWebTokenApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JWTWebTokenApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static User user=new User();
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
           _configuration = configuration;
        }

        [HttpPost("register")]
        public ActionResult<User> Register(UserDto userDto)
        {
            string passwordHash=BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            user.Username=userDto.Username;
            user.PasswordHash=passwordHash; 
            return Ok(user);
        }

        [HttpPost("login")]
        public ActionResult<User> Login(UserDto userDto)
        {
            if (user.Username != userDto.Username) return BadRequest("User not found!");
            if (!BCrypt.Net.BCrypt.Verify(userDto.Password,user.PasswordHash)) return BadRequest("Wrong password!");

            string token = CreateToken(user);

            return Ok(token);
        }
        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
              new Claim(ClaimTypes.Name,user.Username),
            };

            var keyCode = new byte[64]; // 64 bytes = 512 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyCode);
            }

            string secretKey = Convert.ToBase64String(keyCode);

            var securityKey = Convert.FromBase64String(secretKey);

            var key= new SymmetricSecurityKey(securityKey);

            //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Token").Value!));

            var creds= new SigningCredentials(key,SecurityAlgorithms.HmacSha512Signature);

            var token =new JwtSecurityToken(
                 claims :claims,
                 expires:DateTime.Now.AddDays(1),
                 signingCredentials:creds
                );

            var jwt= new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
