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
        var userDto = _mapper.Map<UserDto>(user);

        return Ok(ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto(token, userDto)));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(LoginDto dto)
    {
        var users = await _unitOfWork.Users.FindAsync(u => u.Email == dto.Email);
        var user = users.FirstOrDefault();

        if (user == null || !_authService.VerifyPassword(dto.Password, user.PasswordHash))
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Invalid email or password."));

        var token = _authService.GenerateJwtToken(user);
        var userDto = _mapper.Map<UserDto>(user);

        return Ok(ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto(token, userDto)));
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
}
