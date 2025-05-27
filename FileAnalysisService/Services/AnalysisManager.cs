using FileAnalysisService.Data;
using FileAnalysisService.Models;
using Microsoft.EntityFrameworkCore;
using Common.Models;
using System.Net.Http.Json;

namespace FileAnalysisService.Services;

public class AnalysisManager
{
    private readonly AnalysisDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _storagePath;
    private readonly string _rapidApiKey;

    public AnalysisManager(AnalysisDbContext db, IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _storagePath = config["StoragePath"] ?? "./analysis";
        _rapidApiKey = config["RapidAPI:Key"] ?? string.Empty;
        Directory.CreateDirectory(_storagePath);
    }
    public async Task<FileAnalysisResult> AnalyzeAsync(Guid fileId, byte[] content)
    {
        var text = System.Text.Encoding.UTF8.GetString(content);
        var wordCount = text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
        var paragraphCount = text.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries).Length;
        var characterCount = text.Length;
        var wordCloudImageLocation = await GenerateWordCloudAsync(text);
        var entity = new AnalysisEntity
        {
            Id = fileId,
            Location = Path.Combine(_storagePath, fileId.ToString()),
            WordCount = wordCount,
            ParagraphCount = paragraphCount,
            CharacterCount = characterCount,
            WordCloudImageLocation = wordCloudImageLocation
        };
        try
        {
            Console.WriteLine($"Attempting to write file to: {entity.Location}");
            await File.WriteAllBytesAsync(entity.Location, content);
            Console.WriteLine($"Successfully wrote file to: {entity.Location}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing file {entity.Location}: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            throw; // Rethrow to see the full exception in logs
        }
        _db.Analyses.Add(entity);
        await _db.SaveChangesAsync();
        return new FileAnalysisResult
        {
            Id = entity.Id,
            Location = entity.Location,
            WordCount = entity.WordCount,
            ParagraphCount = entity.ParagraphCount,
            CharacterCount = entity.CharacterCount,
            WordCloudImageLocation = entity.WordCloudImageLocation
        };
    }
    public async Task<FileAnalysisResult?> GetAnalysisAsync(Guid id)
    {
        var entity = await _db.Analyses.FindAsync(id);
        if (entity == null) return null;
        return new FileAnalysisResult
        {
            Id = entity.Id,
            Location = entity.Location,
            WordCount = entity.WordCount,
            ParagraphCount = entity.ParagraphCount,
            CharacterCount = entity.CharacterCount,
            WordCloudImageLocation = entity.WordCloudImageLocation
        };
    }
    public async Task<byte[]?> GetWordCloudImageAsync(string location)
    {
        if (!File.Exists(location)) return null;
        var ext = Path.GetExtension(location).ToLower();
        if (ext == ".png")
        {
            return await File.ReadAllBytesAsync(location);
        }
        // иначе считаем, что это текстовый файл
        var text = await File.ReadAllTextAsync(location);
        var client = _httpClientFactory.CreateClient();
        var url = "https://quickchart.io/wordcloud";
        var response = await client.PostAsJsonAsync(url, new {
            text,
            format = "png",
            width = 600,
            height = 400,
            fontScale = 15,
            removeStopwords = true
        });
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsByteArrayAsync();
    }
    public async Task<byte[]?> GetWordCloudImageFromContentAsync(Guid id, byte[] content)
    {
        try
        {
            Console.WriteLine($"Attempting to generate word cloud for id: {id}");
            var text = System.Text.Encoding.UTF8.GetString(content);
            Console.WriteLine($"Successfully read content as text. Length: {text.Length}");
            var imageLocation = await GenerateWordCloudAsync(text);
            Console.WriteLine($"GenerateWordCloudAsync returned location: {imageLocation}");
            if (string.IsNullOrEmpty(imageLocation) || !File.Exists(imageLocation)) 
            {
                Console.WriteLine("Generated image location is null, empty, or file does not exist.");
                return null;
            }
            Console.WriteLine($"Reading generated image from: {imageLocation}");
            return await File.ReadAllBytesAsync(imageLocation);
        }
        catch (Exception ex)
        {
            // Логирование ошибки, если потребуется
            Console.WriteLine($"Error generating word cloud from content for id {id}: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            return null;
        }
    }
    private async Task<string> GenerateWordCloudAsync(string text)
    {
        var client = _httpClientFactory.CreateClient();
        var url = "https://quickchart.io/wordcloud";
        
        var requestData = new
        {
            text = text,
            format = "png",
            width = 600,
            height = 600,
            fontFamily = "Roboto",
            loadGoogleFonts = "Roboto",
            fontScale = 30,
            removeStopwords = true,
            backgroundColor = "white",
            minFontSize = 5,
            maxFontSize = 100,
            rotation = "random",
            colors = new[] { "#FF0000" },
            cleanWords = false
        };
        Console.WriteLine($"DEBUG: Sending text to QuickChart.io: '{text}'");
        try 
        {
            var response = await client.PostAsJsonAsync(url, requestData);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error from QuickChart.io: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error content: {errorContent}");
                return null;
            }

            var contentType = response.Content.Headers.ContentType?.MediaType;
            Console.WriteLine($"Response content type: {contentType}");

            var imageBytes = await response.Content.ReadAsByteArrayAsync();
            var imageLocation = Path.Combine(_storagePath, Guid.NewGuid() + ".png");
            await File.WriteAllBytesAsync(imageLocation, imageBytes);
            return imageLocation;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating word cloud: {ex.Message}");
            return null;
        }
    }
    private async Task<string> GenerateWordCloudSvgAsync(string text)
    {
        var client = _httpClientFactory.CreateClient();
        var url = "https://quickchart.io/wordcloud";
        var requestData = new
        {
            text = text,
            format = "svg",
            width = 600,
            height = 600,
            fontScale = 15,
            removeStopwords = true,
            backgroundColor = "white",
            minFontSize = 10,
            maxFontSize = 50,
            rotation = "random",
            colors = new[] { "#1f77b4", "#ff7f0e", "#2ca02c", "#d62728", "#9467bd", "#8c564b", "#e377c2", "#7f7f7f", "#bcbd22", "#17becf" }
        };
        try
        {
            var response = await client.PostAsJsonAsync(url, requestData);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error from QuickChart.io: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error content: {errorContent}");
                return null;
            }
            var svg = await response.Content.ReadAsStringAsync();
            var imageLocation = Path.Combine(_storagePath, Guid.NewGuid() + ".svg");
            await File.WriteAllTextAsync(imageLocation, svg);
            return imageLocation;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating word cloud SVG: {ex.Message}");
            return null;
        }
    }
    public async Task<string?> GetWordCloudSvgFromContentAsync(Guid id, byte[] content)
    {
        try
        {
            var text = System.Text.Encoding.UTF8.GetString(content);
            var imageLocation = await GenerateWordCloudSvgAsync(text);
            if (string.IsNullOrEmpty(imageLocation) || !File.Exists(imageLocation))
            {
                return null;
            }
            return await File.ReadAllTextAsync(imageLocation);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating word cloud SVG from content for id {id}: {ex.Message}");
            return null;
        }
    }
} 