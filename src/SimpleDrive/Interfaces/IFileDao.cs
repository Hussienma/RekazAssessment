using SimpleDrive.Entities;

namespace SimpleDrive.Interfaces;

public interface IFileMetadataDao
{
    Task<FileMetadata> GetById(string id);

    Task Create(FileMetadata file);
}

