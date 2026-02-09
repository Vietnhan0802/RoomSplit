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

        var from = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddMonths(1).AddTicks(-1);
        var transactions = await _unitOfWork.Transactions.GetByUserIdAndDateRangeAsync(userId, from, to);
        var expenseTransactions = transactions.Where(t => t.Type == TransactionType.Expense).ToList();

        var dtos = budgets.Select(b =>
        {
            var dto = _mapper.Map<BudgetDto>(b);
            var spent = expenseTransactions
                .Where(t => t.ExpenseCategory == b.ExpenseCategory)
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
            ExpenseCategory = (ExpenseCategory)dto.ExpenseCategory,
            MonthlyLimit = dto.MonthlyLimit,
            Month = dto.Month,
            Year = dto.Year
        };

        await _unitOfWork.Budgets.AddAsync(budget);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBudgets), new { month = dto.Month, year = dto.Year },
            ApiResponse<BudgetDto>.Ok(_mapper.Map<BudgetDto>(budget)));
    }
}
