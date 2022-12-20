using Microsoft.AspNetCore.Mvc;
using JwtAuth.Models;
using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace JwtAuth.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        public static User User = new User();
        private readonly IConfiguration _configuration;
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserRegister user)
        {
            CreatePasswordHash(user.Password, out byte[] passwordHash, out byte[] passwordSalt);

            User.UserName = user.UserName;
            User.PasswordHash = passwordHash;
            User.PasswordSalt = passwordSalt;

            return Ok(User);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserRegister user)
        {
            if(!String.Equals(User.UserName, user.UserName) || !VerifyPasswordHash(user.Password, User.PasswordHash, User.PasswordSalt))
                return BadRequest("Username or Password Invalid!");

            return Ok( CreateToken(User));
        }

        [HttpGet("secret"), Authorize]
        public async Task<List<string>> Secret(){
            return new List<string>{
                string.Empty,
                string.Empty
            };
        }

        private string CreateToken(User user){

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, User.UserName)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes( _configuration.GetSection("AppSettings:Token").Value));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: cred
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)); 
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using(var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

    }
}