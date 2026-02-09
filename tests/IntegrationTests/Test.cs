namespace IntegrationTests;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimpleDrive.Data;
using SimpleDrive.DTOs;
using SimpleDrive.Entities;
using Xunit;

public class SimpleDriveIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly SqliteConnection _metaConnection;
    private readonly SqliteConnection _fileConnection;

    public SimpleDriveIntegrationTests(WebApplicationFactory<Program> factory)
    {
       _metaConnection = new SqliteConnection("DataSource=:memory:");
        _fileConnection = new SqliteConnection("DataSource=:memory:");
        _metaConnection.Open();
        _fileConnection.Open();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                RemoveService<DbContextOptions<AppDbContext>>(services);
                RemoveService<DbContextOptions<FileDbContext>>(services);

                services.AddDbContext<AppDbContext>(options => options.UseSqlite(_metaConnection));
                services.AddDbContext<FileDbContext>(options => options.UseSqlite(_fileConnection));
            });
        });

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        scope.ServiceProvider.GetRequiredService<FileDbContext>().Database.EnsureCreated();
    }
    
    private static void RemoveService<T>(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor != null) services.Remove(descriptor);
    }

    public void Dispose()
    {
        _metaConnection.Close();
        _fileConnection.Close();
    }

    [Fact]
    public async Task Authenticate_ReturnsJwtToken_WhenNameProvided()
    {
        var name = "TestUser";

        var response = await _client.PostAsync($"/v1/auth?name={name}", null);

        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task UploadFile_ReturnsUnauthorized_WhenNoTokenProvided()
    {
        var request = new FileUploadRequest();

        var response = await _client.PostAsJsonAsync("/v1/blobs", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task FullFlow_UploadAndRetrieve_ReturnsSuccess()
    {
        var authResponse = await _client.PostAsync("/v1/auth?name=IntegrationTester", null);
        var token = await authResponse.Content.ReadAsStringAsync();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var uploadRequest = new FileUploadRequest
        {
            Id = "testfile.txt",
            Data = "SGVsbG8gV29ybGQ="
        };

        var uploadResponse = await _client.PostAsJsonAsync("/v1/blobs", uploadRequest);

        Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);

        if (uploadResponse.IsSuccessStatusCode)
        {
            var fileMetadata = await uploadResponse.Content.ReadFromJsonAsync<FileMetadata>();
            Assert.NotNull(fileMetadata);
            Assert.Equal(uploadRequest.Id, fileMetadata.Id);
            
            var getResponse = await _client.GetAsync($"/v1/blobs/{fileMetadata.Id}");
            var fileGetResponse = await getResponse.Content.ReadFromJsonAsync<FileGetResponse>();

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            Assert.NotNull(fileGetResponse);
            Assert.Equal(uploadRequest.Id, fileGetResponse.Id);
            Assert.Equal(uploadRequest.Data, uploadRequest.Data);
        }
    }
}