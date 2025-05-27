using System;

namespace FileStorageService.Models;

public class FileEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Hash { get; set; }
    public string Location { get; set; }
} 