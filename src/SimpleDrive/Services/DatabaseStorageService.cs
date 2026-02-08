using SimpleDrive.Common;
using SimpleDrive.DTOs;
using SimpleDrive.Entities;
using SimpleDrive.Interfaces;
using SimpleDrive.Utils;

namespace SimpleDrive.Services;

public class DatabaseStorageService : IStorageService
{
    IFileRecordDao _recordDao;
    IFileMetadataDao _metadataDao;

    public DatabaseStorageService(IFileRecordDao recordDao, IFileMetadataDao metadataDao)
    {
        _recordDao = recordDao;
        _metadataDao = metadataDao;
    }

    public async Task<Result<FileGetResponse>> GetFileById(string id)
    {
        var fileMetadata = await _metadataDao.GetById(id);
        if (fileMetadata is null) return Result<FileGetResponse>.Failure("File not found");

        var fileRecord = await _recordDao.GetById(id);
        if (fileRecord is null) return Result<FileGetResponse>.Failure("File data not found in storage");

        FileGetResponse file = new FileGetResponse()
        {
            Id = fileMetadata.Id,
            Create_At = fileMetadata.CreatedAt,
            Size = fileMetadata.Size,
            Data = fileRecord.Data
        };

        return Result<FileGetResponse>.Ok(file);
    }

    public async Task<Result<FileMetadata>> UploadFileAsync(FileUploadRequest request)
    {
        Result<FileMetadata> validationResult = Validation.ValidateUploadRequest(request);

        if (!validationResult.Success) return validationResult;

        FileMetadata file = await _metadataDao.GetById(request.Id);

        if (file != null) return Result<FileMetadata>.Failure("ID is already in use");

        FileRecord fileRecord = new FileRecord
        {
          Id = request.Id,
          Data = request.Data  
        };

        file = new FileMetadata
        {
          Id = request.Id,
          Size = request.Data.Length,  
        };
        
        await _recordDao.Create(fileRecord);
        await _metadataDao.Create(file);

        return Result<FileMetadata>.Ok(file);
    }
}