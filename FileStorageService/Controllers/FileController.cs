using Microsoft.AspNetCore.Mvc;
using FileStorageService.Services;

namespace FileStorageService.Controllers;

[ApiController]
[Route("[controller]")]
public class FileController : ControllerBase
{
    private readonly FileStorageManager _manager;
    public FileController(FileStorageManager manager)
    {
        _manager = manager;
    }
    [HttpPost("store")]
    [ProducesResponseType(typeof(Common.Models.FileMetadata), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Store([FromForm] FileUploadDto dto)
    {
        var file = dto.File;
        var meta = await _manager.SaveFileAsync(file.OpenReadStream(), file.FileName);
        return Ok(meta);
    }
    [HttpGet("download")]
    public async Task<IActionResult> Download([FromQuery] Guid id)
    {
        var content = await _manager.GetFileContentAsync(id);
        if (content == null) return NotFound();
        return File(content, "application/octet-stream");
    }
    [HttpGet("metadata")]
    public async Task<IActionResult> Metadata([FromQuery] Guid id)
    {
        var meta = await _manager.GetFileMetadataAsync(id);
        if (meta == null) return NotFound();
        return Ok(meta);
    }
} 