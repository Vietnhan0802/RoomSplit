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

    [HttpGet("status")]
    public async Task<ActionResult<ApiResponse<List<BudgetStatusDto>>>> GetBudgetStatus(
        [FromQuery] int month, [FromQuery] int year)
    {
        var userId = GetUserId();
        var budgets = await _unitOfWork.Budgets.FindAsync(
            b => b.UserId == userId && b.Month == month && b.Year == year);

        var from = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddMonths(1).AddTicks(-1);
        var transactions = await _unitOfWork.Transactions.GetByUserIdAndDateRangeAsync(userId, from, to);
        var expenses = transactions.Where(t => t.Type == TransactionType.Expense).ToList();

        var today = DateTime.UtcNow;
        var isCurrentMonth = today.Month == month && today.Year == year;
        var dayOfMonth = isCurrentMonth ? Math.Max(1, today.Day) : DateTime.DaysInMonth(year, month);
        var daysInMonth = DateTime.DaysInMonth(year, month);

        var statusList = budgets.Select(b =>
        {
            var spent = expenses.Where(t => t.ExpenseCategory == b.ExpenseCategory).Sum(t => t.Amount);
            var dailyAvg = dayOfMonth > 0 ? spent / dayOfMonth : 0;
            var projected = isCurrentMonth ? dailyAvg * daysInMonth : spent;
            var percentUsed = b.MonthlyLimit > 0 ? (double)(spent / b.MonthlyLimit * 100) : 0;

            return new BudgetStatusDto(
                b.Id,
                b.ExpenseCategory.ToString(),
                b.MonthlyLimit,
                spent,
                b.Month,
                b.Year,
                Math.Round(dailyAvg, 0),
                Math.Round(projected, 0),
                spent > b.MonthlyLimit,
                b.MonthlyLimit - spent,
                Math.Round(percentUsed, 1));
        }).ToList();

        return Ok(ApiResponse<List<BudgetStatusDto>>.Ok(statusList));
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

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<BudgetDto>>> UpdateBudget(Guid id, UpdateBudgetDto dto)
    {
        var userId = GetUserId();
        var budget = await _unitOfWork.Budgets.GetByIdAsync(id);

        if (budget == null || budget.UserId != userId)
            return NotFound(ApiResponse.Fail("Budget not found."));

        budget.ExpenseCategory = (ExpenseCategory)dto.ExpenseCategory;
        budget.MonthlyLimit = dto.MonthlyLimit;

        await _unitOfWork.Budgets.UpdateAsync(budget);
        await _unitOfWork.SaveChangesAsync();

        return Ok(ApiResponse<BudgetDto>.Ok(_mapper.Map<BudgetDto>(budget)));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteBudget(Guid id)
    {
        var userId = GetUserId();
        var budget = await _unitOfWork.Budgets.GetByIdAsync(id);

        if (budget == null || budget.UserId != userId)
            return NotFound(ApiResponse.Fail("Budget not found."));

        await _unitOfWork.Budgets.DeleteAsync(budget);
        await _unitOfWork.SaveChangesAsync();

        return Ok(ApiResponse.Ok("Budget deleted."));
    }
}
