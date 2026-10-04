using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PEWS.Models;
using PEWS.ViewModels.Api;
using System.Security.Claims;

namespace PEWS.Controllers.Api;

[ApiController]
[Route("api/profile")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ApiProfileController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ApiProfileController(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        return Ok(new ProfileResponse
        {
            Id          = user.Id,
            FirstName   = user.FirstName,
            LastName    = user.LastName,
            Email       = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Address     = user.Address,
            City        = user.City,
            Region      = user.Region,
            CreatedAt   = user.CreatedAt
        });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.Address = request.Address;
        user.City = request.City;
        user.Region = request.Region;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return Ok(new ProfileResponse
        {
            Id          = user.Id,
            FirstName   = user.FirstName,
            LastName    = user.LastName,
            Email       = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Address     = user.Address,
            City        = user.City,
            Region      = user.Region,
            CreatedAt   = user.CreatedAt
        });
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return Ok(new { message = "Password changed successfully." });
    }
}
