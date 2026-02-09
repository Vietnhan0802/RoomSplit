using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomSplit.API.DTOs;
using RoomSplit.Core.Interfaces;

namespace RoomSplit.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;

    public FilesController(IFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost("upload/{folder}")]
    public async Task<ActionResult<ApiResponse<string>>> Upload(IFormFile file, string folder)
    {
        if (folder != "receipts" && folder != "avatars")
            return BadRequest(ApiResponse<string>.Fail("Invalid upload folder."));

        var url = await _fileService.UploadFileAsync(file, folder);
        return Ok(ApiResponse<string>.Ok(url));
    }
}
