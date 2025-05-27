using Microsoft.EntityFrameworkCore;
using FileStorageService.Models;

namespace FileStorageService.Data;

public class FileStorageDbContext : DbContext
{
    public FileStorageDbContext(DbContextOptions<FileStorageDbContext> options) : base(options) { }
    public DbSet<FileEntity> Files { get; set; }
} 