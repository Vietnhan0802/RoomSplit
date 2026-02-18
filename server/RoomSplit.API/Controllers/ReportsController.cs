using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomSplit.API.DTOs;
using RoomSplit.API.DTOs.Report;
using RoomSplit.API.DTOs.Transaction;
using RoomSplit.Core.Enums;
using RoomSplit.Core.Interfaces;
using System.Security.Claims;

namespace RoomSplit.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ReportsController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponse<ReportOverviewDto>>> GetOverview(
        [FromQuery] int month, [FromQuery] int year)
    {
        var userId = GetUserId();

        var from = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddMonths(1).AddTicks(-1);
        var transactions = (await _unitOfWork.Transactions.GetByUserIdAndDateRangeAsync(userId, from, to)).ToList();

        var prevFrom = from.AddMonths(-1);
        var prevTo = from.AddTicks(-1);
        var prevTransactions = (await _unitOfWork.Transactions.GetByUserIdAndDateRangeAsync(userId, prevFrom, prevTo)).ToList();

        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var prevIncome = prevTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var prevExpense = prevTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        var netSavings = totalIncome - totalExpense;
        var savingsPercent = totalIncome > 0 ? Math.Round(netSavings / totalIncome * 100, 1) : 0;
        var incomeChange = prevIncome > 0 ? Math.Round((totalIncome - prevIncome) / prevIncome * 100, 1) : 0;
        var expenseChange = prevExpense > 0 ? Math.Round((totalExpense - prevExpense) / prevExpense * 100, 1) : 0;

        var topExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .OrderByDescending(t => t.Amount)
            .Take(5)
            .ToList();

        return Ok(ApiResponse<ReportOverviewDto>.Ok(new ReportOverviewDto(
            totalIncome, totalExpense, netSavings, savingsPercent,
            prevIncome, prevExpense, incomeChange, expenseChange,
            _mapper.Map<List<TransactionDto>>(topExpenses))));
    }

    [HttpGet("category-breakdown")]
    public async Task<ActionResult<ApiResponse<List<CategoryBreakdownItemDto>>>> GetCategoryBreakdown(
        [FromQuery] int month, [FromQuery] int year)
    {
        var userId = GetUserId();

        var from = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddMonths(1).AddTicks(-1);
        var transactions = (await _unitOfWork.Transactions.GetByUserIdAndDateRangeAsync(userId, from, to)).ToList();

        var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        var breakdown = transactions
            .Where(t => t.Type == TransactionType.Expense && t.ExpenseCategory.HasValue)
            .GroupBy(t => t.ExpenseCategory!.Value)
            .Select(g => new CategoryBreakdownItemDto(
                Category: g.Key.ToString(),
                Amount: g.Sum(t => t.Amount),
                Count: g.Count(),
                Percentage: totalExpense > 0 ? Math.Round(g.Sum(t => t.Amount) / totalExpense * 100, 1) : 0,
                Transactions: _mapper.Map<List<TransactionDto>>(g.OrderByDescending(t => t.Amount).ToList())))
            .OrderByDescending(c => c.Amount)
            .ToList();

        return Ok(ApiResponse<List<CategoryBreakdownItemDto>>.Ok(breakdown));
    }

    [HttpGet("monthly-trend")]
    public async Task<ActionResult<ApiResponse<List<MonthlyTrendItemDto>>>> GetMonthlyTrend(
        [FromQuery] int months = 6)
    {
        var userId = GetUserId();
        var now = DateTime.UtcNow;
        var result = new List<MonthlyTrendItemDto>();

        for (int i = months - 1; i >= 0; i--)
        {
            var date = now.AddMonths(-i);
            var from = new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = from.AddMonths(1).AddTicks(-1);

            var transactions = (await _unitOfWork.Transactions.GetByUserIdAndDateRangeAsync(userId, from, to)).ToList();

            var income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var expense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

            result.Add(new MonthlyTrendItemDto(from.Month, from.Year, income, expense, income - expense));
        }

        return Ok(ApiResponse<List<MonthlyTrendItemDto>>.Ok(result));
    }

    [HttpGet("daily-spending")]
    public async Task<ActionResult<ApiResponse<DailySpendingResponseDto>>> GetDailySpending(
        [FromQuery] int month, [FromQuery] int year)
    {
        var userId = GetUserId();

        var from = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddMonths(1).AddTicks(-1);
        var transactions = (await _unitOfWork.Transactions.GetByUserIdAndDateRangeAsync(userId, from, to)).ToList();

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var days = Enumerable.Range(1, daysInMonth)
            .Select(d =>
            {
                var date = new DateTime(year, month, d, 0, 0, 0, DateTimeKind.Utc);
                var amount = transactions
                    .Where(t => t.Type == TransactionType.Expense && t.Date.Date == date.Date)
                    .Sum(t => t.Amount);
                return new DailySpendingItemDto(date, amount);
            })
            .ToList();

        var daysWithSpending = days.Where(d => d.Amount > 0).ToList();
        var dailyAverage = daysWithSpending.Count > 0 ? Math.Round(daysWithSpending.Average(d => d.Amount), 0) : 0;
        var maxDay = days.OrderByDescending(d => d.Amount).FirstOrDefault();

        return Ok(ApiResponse<DailySpendingResponseDto>.Ok(new DailySpendingResponseDto(
            days, dailyAverage, maxDay?.Date, maxDay?.Amount ?? 0)));
    }

    [HttpGet("comparison")]
    public async Task<ActionResult<ApiResponse<List<CategoryComparisonItemDto>>>> GetComparison(
        [FromQuery] int month, [FromQuery] int year)
    {
        var userId = GetUserId();

        var from = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddMonths(1).AddTicks(-1);
        var transactions = (await _unitOfWork.Transactions.GetByUserIdAndDateRangeAsync(userId, from, to)).ToList();

        var prevFrom = from.AddMonths(-1);
        var prevTo = from.AddTicks(-1);
        var prevTransactions = (await _unitOfWork.Transactions.GetByUserIdAndDateRangeAsync(userId, prevFrom, prevTo)).ToList();

        // Get all expense categories from both months
        var allCategories = transactions
            .Where(t => t.Type == TransactionType.Expense && t.ExpenseCategory.HasValue)
            .Select(t => t.ExpenseCategory!.Value)
            .Union(prevTransactions
                .Where(t => t.Type == TransactionType.Expense && t.ExpenseCategory.HasValue)
                .Select(t => t.ExpenseCategory!.Value))
            .Distinct();

        var comparison = allCategories.Select(cat =>
        {
            var current = transactions
                .Where(t => t.Type == TransactionType.Expense && t.ExpenseCategory == cat)
                .Sum(t => t.Amount);
            var prev = prevTransactions
                .Where(t => t.Type == TransactionType.Expense && t.ExpenseCategory == cat)
                .Sum(t => t.Amount);
            var change = current - prev;
            var pctChange = prev > 0 ? Math.Round(change / prev * 100, 1) : (current > 0 ? 100 : 0);

            return new CategoryComparisonItemDto(cat.ToString(), prev, current, change, pctChange);
        })
        .OrderByDescending(c => c.CurrentMonthAmount)
        .ToList();

        return Ok(ApiResponse<List<CategoryComparisonItemDto>>.Ok(comparison));
    }
}
