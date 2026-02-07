using System.Net;
using SimpleDrive.DTOs;
using SimpleDrive.Entities;

namespace SimpleDrive.Interfaces;

public interface IStorageService
{
    public Task<ServiceResult> UploadFileAsync(string id, string data);

    public Task<string> RetrieveFileAsync(string id);
}