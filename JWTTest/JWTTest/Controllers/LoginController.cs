using JWTTest.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JWTTest.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
    private IConfiguration _configuration;

    public LoginController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    [AllowAnonymous]
    [HttpPost]
    public IActionResult Login(UserLoginModel login)
    {
        IActionResult response = Unauthorized();
        var user= AuthenticateUser(login); 
        
        if (user != null) 
        {
            var tokenString = GenerateJSONWebToken(user);
            response= Ok(new {token = tokenString});
        }
        return response;
    }
    private UserLoginModel AuthenticateUser(UserLoginModel login)
    {
        UserLoginModel user = null;

        if (login.Username.ToLower()=="test")
        {
            user= new UserLoginModel 
            { 
                Username = login.Username,
                EmailAddress=login.EmailAddress,
                DateOfJoing=login.DateOfJoing,
            };
        }
        return user;
    }
    private string GenerateJSONWebToken(UserLoginModel userInfo)
    {

        var keyCode = new byte[64]; // 64 bytes = 512 bits
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(keyCode);
        }

        string secretKey = Convert.ToBase64String(keyCode);
        var key = Convert.FromBase64String(secretKey);
        var securityKey= new SymmetricSecurityKey(key);
        //var securityKey= new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials =  new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha512);

        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub,userInfo.Username),
            new Claim(JwtRegisteredClaimNames.Email,userInfo.EmailAddress),
            new Claim("DateOfJoing",userInfo.DateOfJoing.ToString("yyyy-MM-dd")),
            new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
            _configuration["Jwt:Issuer"],
            claims,
            expires: DateTime.Now.AddMinutes(120),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token); 
    }
}
