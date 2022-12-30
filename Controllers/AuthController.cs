using Microsoft.AspNetCore.Mvc;
using JwtAuth.Models;
using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using JwtAuth.Helpers.Business;

namespace JwtAuth.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        public static User User = new User();
        public static int Id = 0;
        private readonly IConfiguration _configuration;
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(UserCreate user)
        {
            CreatePasswordHash(user.Password, out byte[] passwordHash, out byte[] passwordSalt);

            User.Id = Id++;
            User.UserName = user.UserName;
            User.Role = (int)Enum.Parse(typeof(RolesEnum), user.Role);
            User.PasswordHash = passwordHash;
            User.PasswordSalt = passwordSalt;
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<Object>> Login(UserLogin user)
        {
            if(!String.Equals(User.UserName, user.UserName) || !VerifyPasswordHash(user.Password, User.PasswordHash, User.PasswordSalt))
                return BadRequest("Username or Password Invalid!");

            return Ok( new { Id = User.Id, Token = CreateToken(User)});
        }

        [HttpGet("secret"), Authorize]
        public async Task<IActionResult> Secret(){
            return Ok(new List<string>{"Secret User"});
        }

        [HttpGet("secretAdmin"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> SecretAdmin(){
            return Ok(new List<string>{"Secret Admin"});
        }

        private string CreateToken(User user){

            List<Claim> claims = new List<Claim>
            {
                //Auth Claims
                new Claim(ClaimTypes.Role, Convert.ToString(Enum.GetName(typeof(RolesEnum), User.Role))),        
                //Check App Claims
                new Claim("Id", Convert.ToString(User.Id)),
                new Claim("Role", Convert.ToString(Enum.GetName(typeof(RolesEnum), User.Role)))
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