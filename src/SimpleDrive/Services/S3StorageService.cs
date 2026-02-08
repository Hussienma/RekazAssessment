using SimpleDrive.DTOs;
using SimpleDrive.Common;
using SimpleDrive.Entities;
using SimpleDrive.Interfaces;
using SimpleDrive.Utils;

namespace SimpleDrive.Services;

public class S3StorageService : IStorageService
{
    IFileMetadataDao _dao;
    IS3RequestProvider _s3RequestProvider;
    HttpClient _httpClient;

    public S3StorageService(IFileMetadataDao dao, HttpClient httpClient, IS3RequestProvider authorizationProvider)
    {
        _dao = dao;
        _httpClient = httpClient;
        _s3RequestProvider = authorizationProvider;
    }

    public async Task<Result<FileGetResponse>> GetFileById(string id)
    {
        var fileMetadata = await _dao.GetById(id);

        if (fileMetadata is null) return Result<FileGetResponse>.Failure("File not found");

        var fileRequest = _s3RequestProvider.Get(id);

        var response = await _httpClient.SendAsync(fileRequest);

        if(!response.IsSuccessStatusCode) return Result<FileGetResponse>.Failure("File not found in storage");

        string fileContent = await response.Content.ReadAsStringAsync();
        FileGetResponse file = new FileGetResponse()
        {
            Id = fileMetadata.Id,
            Create_At = fileMetadata.CreatedAt,
            Size = fileMetadata.Size,
            Data = fileContent
        };

        return Result<FileGetResponse>.Ok(file);
    }

    public async Task<Result<FileMetadata>> UploadFileAsync(FileUploadRequest fileUploadRequest)
    {
        Result<FileMetadata> validationResult = Validation.ValidateUploadRequest(fileUploadRequest);

        if(!validationResult.Success) return validationResult;

        FileMetadata file = await _dao.GetById(fileUploadRequest.Id);

        if (file != null) return Result<FileMetadata>.Failure("ID is already in use");

        file = new FileMetadata()
        {
            Id = fileUploadRequest.Id,
            Size = fileUploadRequest.Data.Length,
        };

        var request = _s3RequestProvider.Put(fileUploadRequest.Id, fileUploadRequest.Data);

        var res = await _httpClient.SendAsync(request);

        if (!res.IsSuccessStatusCode)
        {
            string error = await res.Content.ReadAsStringAsync();
            return Result<FileMetadata>.Failure($"{error}");
        }

        await _dao.Create(file);

        return Result<FileMetadata>.Ok(file);
    }
}