using Microsoft.AspNetCore.Mvc;
using FileAnalysisService.Services;
using Microsoft.AspNetCore.Http;

namespace FileAnalysisService.Controllers;

[ApiController]
[Route("[controller]")]
public class AnalysisController : ControllerBase
{
    private readonly AnalysisManager _manager;
    public AnalysisController(AnalysisManager manager)
    {
        _manager = manager;
    }
    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromQuery] Guid id, [FromForm] FileUploadDto dto)
    {
        Console.WriteLine($"Received analyze request for id: {id}");
        var file = dto.File;
        if (file == null || file.Length == 0) return BadRequest("File content is missing.");
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var contentBytes = ms.ToArray();
        var result = await _manager.AnalyzeAsync(id, contentBytes);
        return Ok(result);
    }
    [HttpGet("result")]
    public async Task<IActionResult> Result([FromQuery] Guid id)
    {
        var result = await _manager.GetAnalysisAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }
    public class FileUploadDto
    {
        public IFormFile File { get; set; }
    }
    [HttpPost("wordcloud")]
    public async Task<IActionResult> WordCloud([FromQuery] Guid id, [FromForm] FileUploadDto dto)
    {
        var file = dto.File;
        if (file == null || file.Length == 0) return BadRequest("File content is missing.");
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var contentBytes = ms.ToArray();
        var svg = await _manager.GetWordCloudSvgFromContentAsync(id, contentBytes);
        if (svg == null) return BadRequest();
        Console.WriteLine($"Sending word cloud SVG of size: {svg.Length} characters");
        return Content(svg, "image/svg+xml");
    }
    [HttpPost("wordcloud/svg")]
    public async Task<IActionResult> WordCloudSvg([FromQuery] Guid id, [FromForm] FileUploadDto dto)
    {
        var file = dto.File;
        if (file == null || file.Length == 0) return BadRequest("File content is missing.");
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var contentBytes = ms.ToArray();
        var svg = await _manager.GetWordCloudSvgFromContentAsync(id, contentBytes);
        if (svg == null) return BadRequest();
        return Content(svg, "image/svg+xml");
    }
} 