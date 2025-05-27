using Common.Models;
using System.Net.Http.Json;

namespace ApiGateway.Services;

public class FileStorageClient
{
    private readonly HttpClient _client;
    public FileStorageClient(HttpClient client)
    {
        _client = client;
    }
    public async Task<FileMetadata?> UploadFileAsync(Stream fileStream, string fileName)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(fileStream), "file", fileName);
        var response = await _client.PostAsync("/file/store", content);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<FileMetadata>();
    }
    public async Task<byte[]?> DownloadFileAsync(Guid id)
    {
        var response = await _client.GetAsync($"/file/download?id={id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsByteArrayAsync();
    }
} 