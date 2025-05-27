using Common.Models;
using System.Net.Http.Json;

namespace ApiGateway.Services;

public class FileAnalysisClient
{
    private readonly HttpClient _client;
    public FileAnalysisClient(HttpClient client)
    {
        _client = client;
    }
    public async Task<FileAnalysisResult?> AnalyzeFileAsync(Guid id, byte[] content)
    {
        var response = await _client.PostAsJsonAsync($"/analysis/analyze?id={id}", content);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<FileAnalysisResult>();
    }
    public async Task<FileAnalysisResult?> GetAnalysisAsync(Guid id)
    {
        var response = await _client.GetAsync($"/analysis/result?id={id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<FileAnalysisResult>();
    }
    public async Task<byte[]?> GetWordCloudImageAsync(string location)
    {
        var response = await _client.GetAsync($"/analysis/wordcloud?location={Uri.EscapeDataString(location)}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsByteArrayAsync();
    }
    public async Task<byte[]?> GetWordCloudImageFromContentAsync(Guid id, byte[] content)
    {
        using var multiPartContent = new MultipartFormDataContent();
        multiPartContent.Add(new ByteArrayContent(content), "fileContent", "file.bin");
        var response = await _client.PostAsync($"/analysis/wordcloud?id={id}", multiPartContent);
        
        if (!response.IsSuccessStatusCode) 
        {
            Console.WriteLine($"Error calling FileAnalysisService wordcloud: {response.StatusCode}");
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"FileAnalysisService error content: {errorContent}");
            return null;
        }
        Console.WriteLine($"Successfully received response from FileAnalysisService wordcloud.");
        return await response.Content.ReadAsByteArrayAsync();
    }
} 