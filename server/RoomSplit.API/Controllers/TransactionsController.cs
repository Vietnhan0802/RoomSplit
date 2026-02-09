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
        var transactions = await _unitOfWork.Transactions.FindAsync(t => t.UserId == userId);

        if (month.HasValue && year.HasValue)
        {
            transactions = transactions.Where(
                t => t.TransactionDate.Month == month && t.TransactionDate.Year == year);
        }

        var dtos = _mapper.Map<List<TransactionDto>>(
            transactions.OrderByDescending(t => t.TransactionDate));
        return Ok(ApiResponse<List<TransactionDto>>.Ok(dtos));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> CreateTransaction(CreateTransactionDto dto)
    {
        var userId = GetUserId();
        var transaction = new Transaction
        {
            UserId = userId,
            Type = (TransactionType)dto.Type,
            Amount = dto.Amount,
            Category = (TransactionCategory)dto.Category,
            Description = dto.Description,
            TransactionDate = dto.TransactionDate,
            Note = dto.Note
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
