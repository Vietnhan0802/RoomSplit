using Microsoft.AspNetCore.Http;

namespace RoomSplit.API.DTOs.Task;

public record CompleteTaskAssignmentDto(
    string? Note,
    IFormFile? ProofImage
);
