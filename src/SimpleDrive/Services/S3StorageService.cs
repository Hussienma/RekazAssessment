using SimpleDrive.DTOs;
using SimpleDrive.Entities;
using SimpleDrive.Interfaces;
using SimpleDrive.Storage.S3.Utils;

namespace SimpleDrive.Services;

public class S3StorageService : IStorageService
{
    IFileDao _dao;
    IAuthorizationProvider _authorizationProvider;
    HttpClient _httpClient;

    public S3StorageService(IFileDao dao, HttpClient httpClient, IAuthorizationProvider authorizationProvider)
    {
        _dao = dao;
        _httpClient = httpClient;
        _authorizationProvider = authorizationProvider;
    }

    public Task<string> RetrieveFileAsync(string id)
    {
        throw new NotImplementedException();
    }

    public async Task<ServiceResult> UploadFileAsync(string id, string data)
    {
        if (string.IsNullOrWhiteSpace(id)) return ServiceResult.Failure("ID is required");

        FileMetadata file = await _dao.GetById(id);

        if (file != null) return ServiceResult.Failure("ID is already in use");

        file = new FileMetadata()
        {
            Id = id,
            Size = data.Length,
            CreatedAt = DateTime.Now
        };


        // DateTime timestampUTC = DateTime.UtcNow;
        // string authorizationHeader = _authorizationProvider.GetAuthorizationHeader("PUT", "127.0.0.1:9000", $"/rekaz/{id}", timestampUTC, data);

        // string uri = $"http://127.0.0.1:9000/rekaz/{id}";

        // var contentBytes = System.Text.Encoding.UTF8.GetBytes(data);
        // var content = new ByteArrayContent(contentBytes);

        // HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, uri)
        // {
        //     Content = content
        // };
        // request.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);

        // request.Headers.Host = "127.0.0.1:9000";
        // request.Headers.Add("x-amz-content-sha256", "UNSIGNED-PAYLOAD");
        // request.Headers.Add("x-amz-date", timestampUTC.ToString("yyyyMMddTHHmmssZ"));

        string method = "PUT";
        string host = "localhost:9000"; // Port included for local development
        string bucket = "rekaz";
        string path = $"/{bucket}/{id}";
        DateTime requestTime = DateTime.UtcNow;

        var request = _authorizationProvider.CreateS3Request(method, host,
            path,
            requestTime,
            payload: data,
            region: "us-east-1");

        var res = await _httpClient.SendAsync(request);
        var requestStr = request.ToString();
        Console.WriteLine($"Request: {requestStr}");

        if (!res.IsSuccessStatusCode)
        {
            string error = await res.Content.ReadAsStringAsync();
            return ServiceResult.Failure($"{error}");
        }

        await _dao.Create(file);

        return ServiceResult.Ok();
    }
}