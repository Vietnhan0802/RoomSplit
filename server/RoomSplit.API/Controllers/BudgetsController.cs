using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomSplit.API.DTOs;
using RoomSplit.API.DTOs.Budget;
using RoomSplit.Core.Entities;
using RoomSplit.Core.Enums;
using RoomSplit.Core.Interfaces;
using System.Security.Claims;

namespace RoomSplit.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BudgetsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BudgetsController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<BudgetDto>>>> GetBudgets(
        [FromQuery] int month, [FromQuery] int year)
    {
        var userId = GetUserId();
        var budgets = await _unitOfWork.Budgets.FindAsync(
            b => b.UserId == userId && b.Month == month && b.Year == year);

        var transactions = await _unitOfWork.Transactions.FindAsync(
            t => t.UserId == userId && t.Type == TransactionType.Expense
                && t.TransactionDate.Month == month && t.TransactionDate.Year == year);

        var dtos = budgets.Select(b =>
        {
            var dto = _mapper.Map<BudgetDto>(b);
            var spent = transactions
                .Where(t => t.Category == b.Category)
                .Sum(t => t.Amount);
            return dto with { SpentAmount = spent };
        }).ToList();

        return Ok(ApiResponse<List<BudgetDto>>.Ok(dtos));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BudgetDto>>> CreateBudget(CreateBudgetDto dto)
    {
        var userId = GetUserId();
        var budget = new Budget
        {
            UserId = userId,
            Category = (TransactionCategory)dto.Category,
            LimitAmount = dto.LimitAmount,
            Month = dto.Month,
            Year = dto.Year
        };

        await _unitOfWork.Budgets.AddAsync(budget);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBudgets), new { month = dto.Month, year = dto.Year },
            ApiResponse<BudgetDto>.Ok(_mapper.Map<BudgetDto>(budget)));
    }
}
