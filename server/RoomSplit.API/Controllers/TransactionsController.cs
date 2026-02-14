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
    private readonly IFileService _fileService;

    public TransactionsController(IUnitOfWork unitOfWork, IMapper mapper, IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileService = fileService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetTransactions(
        [FromQuery] TransactionQueryDto query)
    {
        var userId = GetUserId();

        // Determine category filters based on type
        ExpenseCategory? expCat = null;
        IncomeCategory? incCat = null;
        if (query.Category.HasValue)
        {
            if (query.Type == (int)TransactionType.Expense)
                expCat = (ExpenseCategory)query.Category.Value;
            else if (query.Type == (int)TransactionType.Income)
                incCat = (IncomeCategory)query.Category.Value;
        }

        var (items, totalCount) = await _unitOfWork.Transactions.GetPaginatedAsync(
            userId,
            query.Type.HasValue ? (TransactionType)query.Type : null,
            query.StartDate,
            query.EndDate,
            expCat,
            incCat,
            query.Page,
            query.PageSize,
            query.SortBy,
            query.Order);

        var dtos = _mapper.Map<List<TransactionDto>>(items);
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        var result = new
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = totalPages
        };

        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> GetTransaction(Guid id)
    {
        var userId = GetUserId();
        var transaction = await _unitOfWork.Transactions.GetWithImagesAsync(id);

        if (transaction == null || transaction.UserId != userId)
            return NotFound(ApiResponse<TransactionDto>.Fail("Transaction not found."));

        return Ok(ApiResponse<TransactionDto>.Ok(_mapper.Map<TransactionDto>(transaction)));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> CreateTransaction(
        [FromForm] CreateTransactionDto dto,
        [FromForm] List<IFormFile>? images)
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

        // Handle image uploads (max 3)
        if (images != null && images.Count > 0)
        {
            foreach (var image in images.Take(3))
            {
                var (imageUrl, thumbnailUrl) = await _fileService.UploadWithThumbnailAsync(image, "receipts");
                var transactionImage = new TransactionImage
                {
                    TransactionId = transaction.Id,
                    ImageUrl = imageUrl,
                    ThumbnailUrl = thumbnailUrl,
                    OriginalFileName = image.FileName,
                    FileSize = image.Length
                };
                await _unitOfWork.TransactionImages.AddAsync(transactionImage);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        var result = await _unitOfWork.Transactions.GetWithImagesAsync(transaction.Id);
        return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id },
            ApiResponse<TransactionDto>.Ok(_mapper.Map<TransactionDto>(result!)));
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> UpdateTransaction(
        Guid id,
        [FromForm] UpdateTransactionDto dto,
        [FromForm] List<IFormFile>? newImages,
        [FromForm] string? removeImageIds)
    {
        var userId = GetUserId();
        var transaction = await _unitOfWork.Transactions.GetWithImagesAsync(id);

        if (transaction == null || transaction.UserId != userId)
            return NotFound(ApiResponse.Fail("Transaction not found."));

        // Update fields
        transaction.Type = (TransactionType)dto.Type;
        transaction.Amount = dto.Amount;
        transaction.Description = dto.Description;
        transaction.IncomeCategory = dto.IncomeCategory.HasValue ? (IncomeCategory)dto.IncomeCategory.Value : null;
        transaction.ExpenseCategory = dto.ExpenseCategory.HasValue ? (ExpenseCategory)dto.ExpenseCategory.Value : null;
        transaction.Date = dto.Date;
        transaction.Note = dto.Note;
        transaction.Tags = dto.Tags;

        // Remove specified images
        if (!string.IsNullOrEmpty(removeImageIds))
        {
            var idsToRemove = removeImageIds.Split(',').Select(Guid.Parse).ToList();
            var imagesToRemove = transaction.Images.Where(i => idsToRemove.Contains(i.Id)).ToList();
            foreach (var img in imagesToRemove)
            {
                var filesToDelete = new List<string> { img.ImageUrl };
                if (img.ThumbnailUrl != null) filesToDelete.Add(img.ThumbnailUrl);
                await _fileService.DeleteFilesAsync(filesToDelete);
                await _unitOfWork.TransactionImages.DeleteAsync(img);
            }
        }

        // Add new images (respect max 3 total)
        if (newImages != null && newImages.Count > 0)
        {
            var removedCount = string.IsNullOrEmpty(removeImageIds) ? 0 : removeImageIds.Split(',').Length;
            var currentCount = transaction.Images.Count - removedCount;
            var slotsAvailable = Math.Max(0, 3 - currentCount);

            foreach (var image in newImages.Take(slotsAvailable))
            {
                var (imageUrl, thumbnailUrl) = await _fileService.UploadWithThumbnailAsync(image, "receipts");
                var transactionImage = new TransactionImage
                {
                    TransactionId = transaction.Id,
                    ImageUrl = imageUrl,
                    ThumbnailUrl = thumbnailUrl,
                    OriginalFileName = image.FileName,
                    FileSize = image.Length
                };
                await _unitOfWork.TransactionImages.AddAsync(transactionImage);
            }
        }

        await _unitOfWork.Transactions.UpdateAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        var result = await _unitOfWork.Transactions.GetWithImagesAsync(id);
        return Ok(ApiResponse<TransactionDto>.Ok(_mapper.Map<TransactionDto>(result!)));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteTransaction(Guid id)
    {
        var userId = GetUserId();
        var transaction = await _unitOfWork.Transactions.GetWithImagesAsync(id);

        if (transaction == null || transaction.UserId != userId)
            return NotFound(ApiResponse.Fail("Transaction not found."));

        // Delete image files from disk
        var filePaths = new List<string>();
        foreach (var img in transaction.Images)
        {
            filePaths.Add(img.ImageUrl);
            if (img.ThumbnailUrl != null) filePaths.Add(img.ThumbnailUrl);
        }
        if (!string.IsNullOrEmpty(transaction.ImageUrl))
            filePaths.Add(transaction.ImageUrl);

        if (filePaths.Count > 0)
            await _fileService.DeleteFilesAsync(filePaths);

        await _unitOfWork.Transactions.DeleteAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        return Ok(ApiResponse.Ok("Transaction deleted."));
    }

    [HttpGet("calendar")]
    public async Task<ActionResult<ApiResponse<CalendarResponseDto>>> GetCalendar(
        [FromQuery] int month, [FromQuery] int year)
    {
        var userId = GetUserId();
        var from = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddMonths(1).AddTicks(-1);

        var transactions = await _unitOfWork.Transactions.GetByUserIdAndDateRangeWithImagesAsync(userId, from, to);
        var txList = transactions.ToList();

        // Group by date
        var groupedDays = txList
            .GroupBy(t => t.Date.Date)
            .Select(g => new CalendarDayDto(
                Date: g.Key,
                TotalIncome: g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                TotalExpense: g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
                TransactionCount: g.Count(),
                HasImages: g.Any(t => t.Images.Any()),
                Transactions: _mapper.Map<List<TransactionDto>>(g.OrderByDescending(t => t.CreatedAt).ToList())))
            .ToDictionary(d => d.Date);

        // Fill in all days of the month
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var allDays = Enumerable.Range(1, daysInMonth)
            .Select(d =>
            {
                var date = new DateTime(year, month, d, 0, 0, 0, DateTimeKind.Utc);
                return groupedDays.TryGetValue(date, out var day)
                    ? day
                    : new CalendarDayDto(date, 0, 0, 0, false, new List<TransactionDto>());
            })
            .ToList();

        var summary = new MonthSummaryDto(
            TotalIncome: txList.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
            TotalExpense: txList.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
            NetAmount: txList.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount)
                     - txList.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
            TransactionCount: txList.Count);

        return Ok(ApiResponse<CalendarResponseDto>.Ok(new CalendarResponseDto(allDays, summary)));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<SummaryResponseDto>>> GetSummary(
        [FromQuery] int month, [FromQuery] int year)
    {
        var userId = GetUserId();

        // Current month
        var from = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddMonths(1).AddTicks(-1);
        var transactions = (await _unitOfWork.Transactions.GetByUserIdAndDateRangeAsync(userId, from, to)).ToList();

        // Previous month for comparison
        var prevFrom = from.AddMonths(-1);
        var prevTo = from.AddTicks(-1);
        var prevTransactions = (await _unitOfWork.Transactions.GetByUserIdAndDateRangeAsync(userId, prevFrom, prevTo)).ToList();

        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var prevIncome = prevTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var prevExpense = prevTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        // Category breakdown (expenses only)
        var byCategory = transactions
            .Where(t => t.Type == TransactionType.Expense && t.ExpenseCategory.HasValue)
            .GroupBy(t => t.ExpenseCategory!.Value)
            .Select(g => new CategorySummaryDto(
                Category: g.Key.ToString(),
                Amount: g.Sum(t => t.Amount),
                Count: g.Count(),
                Percentage: totalExpense > 0 ? Math.Round(g.Sum(t => t.Amount) / totalExpense * 100, 1) : 0))
            .OrderByDescending(c => c.Amount)
            .ToList();

        // Daily trend
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var dailyTrend = Enumerable.Range(1, daysInMonth)
            .Select(d =>
            {
                var date = new DateTime(year, month, d, 0, 0, 0, DateTimeKind.Utc);
                var dayTx = transactions.Where(t => t.Date.Date == date.Date);
                return new DailyTrendDto(
                    Date: date,
                    Income: dayTx.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    Expense: dayTx.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount));
            })
            .ToList();

        // Top 5 expenses
        var topExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .OrderByDescending(t => t.Amount)
            .Take(5)
            .ToList();

        // Month-over-month comparison
        var incomeChange = prevIncome > 0 ? Math.Round((totalIncome - prevIncome) / prevIncome * 100, 1) : 0;
        var expenseChange = prevExpense > 0 ? Math.Round((totalExpense - prevExpense) / prevExpense * 100, 1) : 0;

        return Ok(ApiResponse<SummaryResponseDto>.Ok(new SummaryResponseDto(
            totalIncome,
            totalExpense,
            totalIncome - totalExpense,
            byCategory,
            dailyTrend,
            _mapper.Map<List<TransactionDto>>(topExpenses),
            new ComparisonDto(incomeChange, expenseChange))));
    }
}
