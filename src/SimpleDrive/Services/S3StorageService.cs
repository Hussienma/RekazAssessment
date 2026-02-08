using SimpleDrive.DTOs;
using SimpleDrive.Common;
using SimpleDrive.Entities;
using SimpleDrive.Interfaces;

namespace SimpleDrive.Services;

public class S3StorageService : IStorageService
{
    IFileDao _dao;
    IS3RequestProvider _s3RequestProvider;
    HttpClient _httpClient;

    public S3StorageService(IFileDao dao, HttpClient httpClient, IS3RequestProvider authorizationProvider)
    {
        _dao = dao;
        _httpClient = httpClient;
        _s3RequestProvider = authorizationProvider;
    }

    public async Task<Result<GetFileResponse>> GetFileById(string id)
    {
        var fileMetadata = await _dao.GetById(id);

        if (fileMetadata is null) return Result<GetFileResponse>.Failure("File not found");

        var fileRequest = _s3RequestProvider.Get(id);

        var response = await _httpClient.SendAsync(fileRequest);

        if(!response.IsSuccessStatusCode) return Result<GetFileResponse>.Failure("File not found in bucket");

        string fileContent = await response.Content.ReadAsStringAsync();
        GetFileResponse file = new GetFileResponse()
        {
            Id = fileMetadata.Id,
            CreateAt = fileMetadata.CreatedAt,
            Size = fileMetadata.Size,
            Data = fileContent
        };

        return Result<GetFileResponse>.Ok(file);
    }

    public async Task<Result<FileMetadata>> UploadFileAsync(string id, string data)
    {
        if (string.IsNullOrWhiteSpace(id)) return Result<FileMetadata>.Failure("ID is required");

        FileMetadata file = await _dao.GetById(id);

        if (file != null) return Result<FileMetadata>.Failure("ID is already in use");

        file = new FileMetadata()
        {
            Id = id,
            Size = data.Length,
            CreatedAt = DateTime.Now
        };

        var request = _s3RequestProvider.Put(id, data);

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