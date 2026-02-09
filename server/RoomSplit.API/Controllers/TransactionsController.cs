using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomSplit.API.DTOs;
using RoomSplit.API.DTOs.Transaction;
using RoomSplit.Core.Entities;
using RoomSplit.Core.Enums;
using RoomSplit.Core.Interfaces;
using System.Security.Claims;

namespace RoomSplit.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TransactionsController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TransactionDto>>>> GetTransactions(
        [FromQuery] int? month, [FromQuery] int? year)
    {
        var userId = GetUserId();

        if (month.HasValue && year.HasValue)
        {
            var from = new DateTime(year.Value, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = from.AddMonths(1).AddTicks(-1);
            var filtered = await _unitOfWork.Transactions.GetByUserIdAndDateRangeAsync(userId, from, to);
            return Ok(ApiResponse<List<TransactionDto>>.Ok(_mapper.Map<List<TransactionDto>>(filtered)));
        }

        var transactions = await _unitOfWork.Transactions.GetByUserIdAsync(userId);
        return Ok(ApiResponse<List<TransactionDto>>.Ok(_mapper.Map<List<TransactionDto>>(transactions)));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> CreateTransaction(CreateTransactionDto dto)
    {
        var userId = GetUserId();
        var transactionType = (TransactionType)dto.Type;

        var transaction = new Transaction
        {
            UserId = userId,
            Type = transactionType,
            Amount = dto.Amount,
            Description = dto.Description,
            IncomeCategory = dto.IncomeCategory.HasValue ? (IncomeCategory)dto.IncomeCategory.Value : null,
            ExpenseCategory = dto.ExpenseCategory.HasValue ? (ExpenseCategory)dto.ExpenseCategory.Value : null,
            Date = dto.Date,
            ImageUrl = dto.ImageUrl,
            Note = dto.Note,
            Tags = dto.Tags
        };

        await _unitOfWork.Transactions.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTransactions), null,
            ApiResponse<TransactionDto>.Ok(_mapper.Map<TransactionDto>(transaction)));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteTransaction(Guid id)
    {
        var userId = GetUserId();
        var transaction = await _unitOfWork.Transactions.GetByIdAsync(id);

        if (transaction == null || transaction.UserId != userId)
            return NotFound(ApiResponse.Fail("Transaction not found."));

        await _unitOfWork.Transactions.DeleteAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        return Ok(ApiResponse.Ok("Transaction deleted."));
    }
}
