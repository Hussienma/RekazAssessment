using SimpleDrive.Entities;

namespace SimpleDrive.Interfaces;

public interface IFileDao
{
    Task<FileMetadata> GetById(string id);

    Task Create(FileMetadata file);
}

