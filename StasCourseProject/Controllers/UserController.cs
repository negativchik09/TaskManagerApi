using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StasCourseProject.Auth;

namespace StasCourseProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;

    public UserController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }
    
    [HttpPost("register")]
    [ProducesResponseType(200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var userExists = await _userManager.FindByNameAsync(model.Login);
        if (userExists != null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        IdentityUser user = new()
        {
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Login
        };
        
        var result = await _userManager.CreateAsync(user, model.Password);
        
        if (!result.Succeeded)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        await _userManager.AddToRoleAsync(user, model.Role);
        
        return Ok();
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    public async Task<IActionResult> Login([FromBody] LoginData model)
    {
        var user = await _userManager.FindByNameAsync(model.Login);
        
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return Ok(new LoginResponse()
            {
                Login = model.Login,
                Type = 0
            });
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        return Ok(new LoginResponse()
        {
            Login = user.UserName,
            UserId = user.Id,
            Type = userRoles.Contains(UserRoles.Manager) ? 1 : 2
        });
    }

    [HttpGet("")]
    [ProducesResponseType(typeof(List<UserInList>), 200)]
    public async Task<IActionResult> GetUsers()
    {
        var result = new List<IdentityUser>();
        foreach (var user in _userManager.Users.AsNoTracking())
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(UserRoles.Manager) && roles.Contains(UserRoles.User))
            {
                result.Add(user);
            }
        }

        return Ok(result.Select(x => new UserInList()
        {
            Login = x.UserName,
            UserId = x.Id
        }).ToList());
    }
}