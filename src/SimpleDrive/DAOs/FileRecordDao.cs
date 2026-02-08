using System.Data.Common;
using SimpleDrive.Data;
using SimpleDrive.Entities;
using SimpleDrive.Interfaces;

namespace SimpleDrive.DAOs;

public class FileRecordDao : IFileRecordDao
{
    FileDbContext _db;

    public FileRecordDao(FileDbContext db)
    {
        _db = db;
    }
    
    public async Task Create(FileRecord file)
    {
        await _db.Records.AddAsync(file);
        await _db.SaveChangesAsync();
    }

    public async Task<FileRecord> GetById(string id)
    {
        return await _db.Records.FindAsync(id);
    }

}