using SimpleDrive.DTOs;
using SimpleDrive.Common;
using SimpleDrive.Entities;

namespace SimpleDrive.Interfaces;

public interface IStorageService
{
    public Task<Result<FileMetadata>> UploadFileAsync(string id, string data);

    public Task<Result<GetFileResponse>> GetFileById(string id);
}