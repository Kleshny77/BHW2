using FileStorageService.Data;
using FileStorageService.Models;
using Microsoft.EntityFrameworkCore;
using Common.Models;

namespace FileStorageService.Services;

public class FileStorageManager
{
    private readonly FileStorageDbContext _db;
    private readonly string _storagePath;
    public FileStorageManager(FileStorageDbContext db, IConfiguration config)
    {
        _db = db;
        _storagePath = config["StoragePath"] ?? "./files";
        Directory.CreateDirectory(_storagePath);
    }
    public async Task<FileMetadata> SaveFileAsync(Stream fileStream, string fileName)
    {
        using var ms = new MemoryStream();
        await fileStream.CopyToAsync(ms);
        var bytes = ms.ToArray();
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes));
        var existing = await _db.Files.FirstOrDefaultAsync(f => f.Hash == hash);
        if (existing != null)
        {
            return new FileMetadata { Id = existing.Id, Name = existing.Name, Hash = existing.Hash, Location = existing.Location };
        }
        var id = Guid.NewGuid();
        var location = Path.Combine(_storagePath, id.ToString());
        await File.WriteAllBytesAsync(location, bytes);
        var entity = new FileEntity { Id = id, Name = fileName, Hash = hash, Location = location };
        _db.Files.Add(entity);
        await _db.SaveChangesAsync();
        return new FileMetadata { Id = id, Name = fileName, Hash = hash, Location = location };
    }
    public async Task<FileMetadata?> GetFileMetadataAsync(Guid id)
    {
        var entity = await _db.Files.FindAsync(id);
        if (entity == null) return null;
        return new FileMetadata { Id = entity.Id, Name = entity.Name, Hash = entity.Hash, Location = entity.Location };
    }
    public async Task<byte[]?> GetFileContentAsync(Guid id)
    {
        var entity = await _db.Files.FindAsync(id);
        if (entity == null) return null;
        if (!File.Exists(entity.Location)) return null;
        return await File.ReadAllBytesAsync(entity.Location);
    }
} 