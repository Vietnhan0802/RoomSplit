using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomSplit.API.DTOs;
using RoomSplit.API.DTOs.Auth;
using RoomSplit.Core.Entities;
using RoomSplit.Core.Interfaces;
using System.Security.Claims;

namespace RoomSplit.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;

    public AuthController(IUnitOfWork unitOfWork, IAuthService authService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _mapper = mapper;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register(RegisterDto dto)
    {
        var existingUsers = await _unitOfWork.Users.FindAsync(u => u.Email == dto.Email);
        if (existingUsers.Any())
            return BadRequest(ApiResponse<AuthResponseDto>.Fail("Email already registered."));

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = _authService.HashPassword(dto.Password)
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var token = _authService.GenerateJwtToken(user);
        var refreshToken = await _authService.CreateRefreshTokenAsync(user, GetClientIpAddress());
        var userDto = _mapper.Map<UserDto>(user);

        return Ok(ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto(token, refreshToken.Token, userDto)));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(LoginDto dto)
    {
        var users = await _unitOfWork.Users.FindAsync(u => u.Email == dto.Email);
        var user = users.FirstOrDefault();

        if (user == null || !_authService.VerifyPassword(dto.Password, user.PasswordHash))
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Invalid email or password."));

        var token = _authService.GenerateJwtToken(user);
        var refreshToken = await _authService.CreateRefreshTokenAsync(user, GetClientIpAddress());
        var userDto = _mapper.Map<UserDto>(user);

        return Ok(ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto(token, refreshToken.Token, userDto)));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _unitOfWork.Users.GetByIdAsync(userId);

        if (user == null)
            return NotFound(ApiResponse<UserDto>.Fail("User not found."));

        return Ok(ApiResponse<UserDto>.Ok(_mapper.Map<UserDto>(user)));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Refresh(RefreshTokenDto dto)
    {
        try
        {
            var (accessToken, newRefreshToken) = await _authService
                .RefreshTokenAsync(dto.RefreshToken, GetClientIpAddress());

            // Get user from refresh token
            var validatedToken = await _authService.ValidateRefreshTokenAsync(newRefreshToken);
            if (validatedToken == null)
                return Unauthorized(ApiResponse<AuthResponseDto>
                    .Fail("Invalid refresh token."));

            var user = await _unitOfWork.Users.GetByIdAsync(validatedToken.UserId);
            if (user == null)
                return Unauthorized(ApiResponse<AuthResponseDto>
                    .Fail("User not found."));

            var userDto = _mapper.Map<UserDto>(user);
            return Ok(ApiResponse<AuthResponseDto>.Ok(
                new AuthResponseDto(accessToken, newRefreshToken, userDto)));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail(ex.Message));
        }
    }

    [Authorize]
    [HttpPut("profile")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile(
        [FromForm] UpdateProfileDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _unitOfWork.Users.GetByIdAsync(userId);

        if (user == null)
            return NotFound(ApiResponse<UserDto>.Fail("User not found."));

        await _authService.UpdateProfileAsync(user, dto.FullName, dto.Avatar);

        return Ok(ApiResponse<UserDto>.Ok(_mapper.Map<UserDto>(user)));
    }

    private string GetClientIpAddress()
    {
        var ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ipAddress))
        {
            ipAddress = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        }
        if (string.IsNullOrEmpty(ipAddress))
        {
            ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        }
        return ipAddress ?? "unknown";
    }
}
