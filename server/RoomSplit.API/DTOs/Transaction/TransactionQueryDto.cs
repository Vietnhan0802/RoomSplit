namespace RoomSplit.API.DTOs.Transaction;

public record TransactionQueryDto(
    int? Type = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int? Category = null,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "date",
    string Order = "desc");
