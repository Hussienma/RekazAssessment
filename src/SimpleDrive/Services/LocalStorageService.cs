using SimpleDrive.Common;
using SimpleDrive.DTOs;
using SimpleDrive.Entities;
using SimpleDrive.Interfaces;
using SimpleDrive.Utils;

namespace SimpleDrive.Services;

public class LocalStorageService : IStorageService
{
    private string _directoryPath;
    private IFileSystem _fileSystem;
    private IFileMetadataDao _dao;

    public LocalStorageService(IConfiguration config, IFileSystem fileSystem, IFileMetadataDao dao)
    {
        _directoryPath = config.GetValue<string>("StorageSettings:Local:DirectoryPath")!;
        _fileSystem = fileSystem;
        _dao = dao;
    }

    public async Task<Result<FileGetResponse>> GetFileById(string id)
    {
        var fileMetadata = await _dao.GetById(id);
        if (fileMetadata is null) return Result<FileGetResponse>.Failure("File not found");

        string filePath = Path.Combine(_directoryPath, id);

        if (!_fileSystem.FileExists(filePath)) return Result<FileGetResponse>.Failure("File not found in storage");

        string fileContent = _fileSystem.ReadAllText(filePath);

        FileGetResponse file = new FileGetResponse()
        {
            Id = fileMetadata.Id,
            Create_At = fileMetadata.CreatedAt,
            Size = fileMetadata.Size,
            Data = fileContent
        };

        return Result<FileGetResponse>.Ok(file);
    }

    public async Task<Result<FileMetadata>> UploadFileAsync(FileUploadRequest request)
    {
        Result<FileMetadata> validationResult = Validation.ValidateUploadRequest(request);
        if (!validationResult.Success) return validationResult;

        FileMetadata file = await _dao.GetById(request.Id);
        if (file != null) return Result<FileMetadata>.Failure("ID is already in use");

        string filePath = Path.Combine(_directoryPath, request.Id);
        if (!_fileSystem.DirectoryExists(_directoryPath))
        {
            _fileSystem.CreateDirectory(_directoryPath);
        }

        _fileSystem.WriteAllText(filePath, request.Data);

        file = new FileMetadata()
        {
            Id = request.Id,
            Size = request.Data.Length,
        };
        await _dao.Create(file);

        return Result<FileMetadata>.Ok(file);
    }
}