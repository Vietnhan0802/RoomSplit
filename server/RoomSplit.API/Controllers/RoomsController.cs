using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomSplit.API.DTOs;
using RoomSplit.API.DTOs.Room;
using RoomSplit.Core.Entities;
using RoomSplit.Core.Enums;
using RoomSplit.Core.Interfaces;
using System.Security.Claims;

namespace RoomSplit.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RoomsController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RoomDto>>>> GetMyRooms()
    {
        var userId = GetUserId();
        var memberships = await _unitOfWork.RoomMembers.FindAsync(rm => rm.UserId == userId && rm.IsActive);
        var roomIds = memberships.Select(m => m.RoomId).ToList();
        var rooms = await _unitOfWork.Rooms.FindAsync(r => roomIds.Contains(r.Id));
        var roomDtos = _mapper.Map<List<RoomDto>>(rooms);
        return Ok(ApiResponse<List<RoomDto>>.Ok(roomDtos));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RoomDto>>> CreateRoom(CreateRoomDto dto)
    {
        var userId = GetUserId();
        var room = new Room
        {
            Name = dto.Name,
            Description = dto.Description,
            InviteCode = GenerateInviteCode(),
            CreatedByUserId = userId
        };

        await _unitOfWork.Rooms.AddAsync(room);

        var member = new RoomMember
        {
            RoomId = room.Id,
            UserId = userId,
            Role = RoomRole.Admin
        };
        await _unitOfWork.RoomMembers.AddAsync(member);
        await _unitOfWork.SaveChangesAsync();

        var roomDto = _mapper.Map<RoomDto>(room);
        return CreatedAtAction(nameof(GetMyRooms), ApiResponse<RoomDto>.Ok(roomDto));
    }

    [HttpPost("join/{inviteCode}")]
    public async Task<ActionResult<ApiResponse<RoomDto>>> JoinRoom(string inviteCode)
    {
        var userId = GetUserId();
        var rooms = await _unitOfWork.Rooms.FindAsync(r => r.InviteCode == inviteCode && r.IsActive);
        var room = rooms.FirstOrDefault();

        if (room == null)
            return NotFound(ApiResponse<RoomDto>.Fail("Room not found."));

        var existing = await _unitOfWork.RoomMembers.FindAsync(
            rm => rm.RoomId == room.Id && rm.UserId == userId);
        if (existing.Any())
            return BadRequest(ApiResponse<RoomDto>.Fail("Already a member of this room."));

        var member = new RoomMember
        {
            RoomId = room.Id,
            UserId = userId,
            Role = RoomRole.Member
        };
        await _unitOfWork.RoomMembers.AddAsync(member);
        await _unitOfWork.SaveChangesAsync();

        return Ok(ApiResponse<RoomDto>.Ok(_mapper.Map<RoomDto>(room)));
    }

    private static string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}
