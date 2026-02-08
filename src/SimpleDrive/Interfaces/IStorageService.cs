using SimpleDrive.DTOs;
using SimpleDrive.Common;
using SimpleDrive.Entities;

namespace SimpleDrive.Interfaces;

public interface IStorageService
{
    public Task<Result<FileMetadata>> UploadFileAsync(FileUploadRequest request);

    public Task<Result<FileGetResponse>> GetFileById(string id);
}