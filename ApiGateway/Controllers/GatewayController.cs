using Microsoft.AspNetCore.Mvc;
using ApiGateway.Services;

namespace ApiGateway.Controllers;

[ApiController]
[Route("")]
public class GatewayController : ControllerBase
{
    private readonly FileStorageClient _storageClient;
    private readonly FileAnalysisClient _analysisClient;
    public GatewayController(FileStorageClient storageClient, FileAnalysisClient analysisClient)
    {
        _storageClient = storageClient;
        _analysisClient = analysisClient;
    }
    [HttpPost("upload")]
    [ProducesResponseType(typeof(Common.Models.FileMetadata), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Upload([FromForm] FileUploadDto dto)
    {
        var file = dto.File;
        var meta = await _storageClient.UploadFileAsync(file.OpenReadStream(), file.FileName);
        if (meta == null) return BadRequest();
        return Ok(meta);
    }
    [HttpGet("download")]
    public async Task<IActionResult> Download([FromQuery] Guid id)
    {
        var content = await _storageClient.DownloadFileAsync(id);
        if (content == null) return NotFound();
        return File(content, "application/octet-stream");
    }
    [HttpGet("analysis")]
    public async Task<IActionResult> Analyze([FromQuery] Guid id)
    {
        var content = await _storageClient.DownloadFileAsync(id);
        if (content == null) return NotFound();
        var result = await _analysisClient.AnalyzeFileAsync(id, content);
        if (result == null) return BadRequest();
        return Ok(result);
    }
    [HttpGet("get")]
    public async Task<IActionResult> GetAnalysis([FromQuery] Guid id)
    {
        var result = await _analysisClient.GetAnalysisAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }
    [HttpGet("wordcloud")]
    public async Task<IActionResult> GetWordCloud([FromQuery] Guid id)
    {
        Console.WriteLine($"Received wordcloud request for id: {id}");
        var fileContent = await _storageClient.DownloadFileAsync(id);
        
        if (fileContent == null) 
        {
            Console.WriteLine($"File content is null for id: {id}");
            return NotFound();
        }
        Console.WriteLine($"Successfully downloaded file content for id: {id}. Size: {fileContent.Length} bytes.");
        
        Console.WriteLine($"Calling GetWordCloudImageFromContentAsync for id: {id}");
        var image = await _analysisClient.GetWordCloudImageFromContentAsync(id, fileContent);
        
        if (image == null) 
        {
            Console.WriteLine($"GetWordCloudImageFromContentAsync returned null for id: {id}");
            return BadRequest();
        }
        
        Console.WriteLine($"Received image from GetWordCloudImageFromContentAsync for id: {id}. Size: {image.Length} bytes.");
        return File(image, "image/png");
    }
} 