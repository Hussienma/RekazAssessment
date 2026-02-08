using SimpleDrive.Data;
using SimpleDrive.Entities;
using SimpleDrive.Interfaces;

namespace SimpleDrive.DAOs;

public class FileMetadataDao : IFileMetadataDao
{
    AppDbContext _db;

    public FileMetadataDao(AppDbContext context)
    {
        _db = context;
    }
    public async Task Create(FileMetadata file)
    {
        await _db.Files.AddAsync(file);
        await _db.SaveChangesAsync();
    }

    public async Task<FileMetadata> GetById(string id)
    {
        return await _db.Files.FindAsync(id);
    }
}