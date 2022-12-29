using Microsoft.AspNetCore.Mvc;
using JwtAuth.Helpers.Business;

namespace NET6_WebApi_JWT_Role_Based_Auth.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServicesController : ControllerBase
    {
        [HttpGet("getroles")]
        public async Task<IActionResult> GetRoles()
        {
            return Ok(Enum.GetNames(typeof(RolesEnum)));
        }
    }
}