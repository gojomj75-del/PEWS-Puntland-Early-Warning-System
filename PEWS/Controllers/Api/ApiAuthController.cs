using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PEWS.Models;
using PEWS.Services;
using PEWS.ViewModels.Api;

namespace PEWS.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class ApiAuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;

    public ApiAuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
            return Unauthorized(new { message = "Invalid email or password." });

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
            return Unauthorized(new { message = "Account locked. Try again in 15 minutes." });

        if (!result.Succeeded)
            return Unauthorized(new { message = "Invalid email or password." });

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user, roles);

        return Ok(new AuthResponse
        {
            Token    = token,
            UserId   = user.Id,
            Email    = user.Email!,
            FullName = user.FullName,
            Role     = roles.FirstOrDefault() ?? "Citizen",
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName      = request.Email,
            Email         = request.Email,
            FirstName     = request.FirstName,
            LastName      = request.LastName,
            PhoneNumber   = request.PhoneNumber,
            City          = request.City,
            Region        = request.Region,
            EmailConfirmed = true,
            IsActive      = true,
            CreatedAt     = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await _userManager.AddToRoleAsync(user, "Citizen");
        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user, roles);

        return Ok(new AuthResponse
        {
            Token    = token,
            UserId   = user.Id,
            Email    = user.Email!,
            FullName = user.FullName,
            Role     = "Citizen",
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        });
    }
}
