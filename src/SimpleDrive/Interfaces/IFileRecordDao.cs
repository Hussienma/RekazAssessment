using SimpleDrive.Entities;

namespace SimpleDrive.Interfaces;

public interface IFileRecordDao
{
     Task<FileRecord> GetById(string id);

    Task Create(FileRecord file);
}