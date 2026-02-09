using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomSplit.API.DTOs;
using RoomSplit.API.DTOs.Expense;
using RoomSplit.Core.Entities;
using RoomSplit.Core.Enums;
using RoomSplit.Core.Interfaces;
using System.Security.Claims;

namespace RoomSplit.API.Controllers;

[Authorize]
[ApiController]
[Route("api/rooms/{roomId}/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ExpensesController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ExpenseDto>>>> GetExpenses(Guid roomId)
    {
        var expenses = await _unitOfWork.Expenses.FindAsync(e => e.RoomId == roomId);
        var dtos = _mapper.Map<List<ExpenseDto>>(expenses.OrderByDescending(e => e.ExpenseDate));
        return Ok(ApiResponse<List<ExpenseDto>>.Ok(dtos));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ExpenseDto>>> CreateExpense(Guid roomId, CreateExpenseDto dto)
    {
        var userId = GetUserId();
        var expense = new Expense
        {
            RoomId = roomId,
            PaidByUserId = userId,
            Description = dto.Description,
            Amount = dto.Amount,
            Category = (ExpenseCategory)dto.Category,
            SplitType = (SplitType)dto.SplitType,
            ExpenseDate = dto.ExpenseDate,
            Note = dto.Note
        };

        await _unitOfWork.Expenses.AddAsync(expense);

        if (dto.SplitType == (int)SplitType.Equal)
        {
            var members = await _unitOfWork.RoomMembers.FindAsync(rm => rm.RoomId == roomId && rm.IsActive);
            var memberCount = members.Count();
            var splitAmount = dto.Amount / memberCount;

            foreach (var member in members)
            {
                var split = new ExpenseSplit
                {
                    ExpenseId = expense.Id,
                    UserId = member.UserId,
                    Amount = splitAmount,
                    IsPaid = member.UserId == userId
                };
                await _unitOfWork.ExpenseSplits.AddAsync(split);
            }
        }
        else if (dto.Splits != null)
        {
            foreach (var s in dto.Splits)
            {
                var split = new ExpenseSplit
                {
                    ExpenseId = expense.Id,
                    UserId = s.UserId,
                    Amount = s.Amount ?? 0,
                    Percentage = s.Percentage,
                    IsPaid = s.UserId == userId
                };
                await _unitOfWork.ExpenseSplits.AddAsync(split);
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(GetExpenses), new { roomId },
            ApiResponse<ExpenseDto>.Ok(_mapper.Map<ExpenseDto>(expense)));
    }
}
