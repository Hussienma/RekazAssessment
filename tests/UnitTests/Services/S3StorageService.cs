
using System.Net;
using Castle.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using SimpleDrive.DTOs;
using SimpleDrive.Entities;
using SimpleDrive.Interfaces;
using SimpleDrive.Services;

public class S3StorageServiceTests
{
    private readonly Mock<IFileMetadataDao> _mockDao;
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly Mock<IS3RequestProvider> _mockRequestProvider;
    private readonly S3StorageService _service;

    public S3StorageServiceTests()
    {
        _mockDao = new Mock<IFileMetadataDao>();
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _mockRequestProvider = new Mock<IS3RequestProvider>();

        var httpClient = new HttpClient(_mockHttpHandler.Object);
        _service = new S3StorageService(_mockDao.Object, httpClient, _mockRequestProvider.Object);
    }

    [Fact]
    public async Task GetFileById_ReturnsFailure_WhenFileDoesNotExist()
    {
        _mockDao.Setup(d => d.GetById("123"))!.ReturnsAsync((FileMetadata)null);

        var result = await _service.GetFileById("123");

        Assert.False(result.Success);
        Assert.Equal("File not found", result.Message);

        _mockDao.Verify(d => d.GetById(It.IsAny<string>()), Times.Once);
        _mockRequestProvider.Verify(d => d.Get(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetFileById_ReturnsFailure_WhenFileIsNotInBucket()
    {
        string id = "123";
        var request = new HttpRequestMessage(HttpMethod.Get, $"localhost:9000/{id}");
        _mockDao.Setup(d => d.GetById(id)).ReturnsAsync(new FileMetadata());
        _mockRequestProvider.Setup(d => d.Get(id)).Returns(request);
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", request, ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
            });

        var response = await _service.GetFileById(id);

        Assert.False(response.Success);
        Assert.Null(response.Value);

        _mockRequestProvider.Verify(d => d.Get(id), Times.Once);
    }

    [Fact]
    public async Task GetFileByID_ReturnsOk_WhenFileExists()
    {
        string id = "123";
        var request = new HttpRequestMessage(HttpMethod.Get, $"localhost:9000/{id}");
        _mockDao.Setup(d => d.GetById(id)).ReturnsAsync(new FileMetadata());
        _mockRequestProvider.Setup(d => d.Get(id)).Returns(request);
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", request, ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
            });

        var response = await _service.GetFileById(id);

        Assert.True(response.Success);
        Assert.NotNull(response.Value);

        _mockRequestProvider.Verify(d => d.Get(id), Times.Once);
    }

    [Fact]
    public async Task UploadFileAsync_ReturnsFailure_WhenIdAlreadyExists()
    {
        var request = new FileUploadRequest { Id = "existing-id", Data = "Data" };

        _mockDao.Setup(d => d.GetById(request.Id)).ReturnsAsync(new FileMetadata());

        var result = await _service.UploadFileAsync(request);

        Assert.False(result.Success);
        Assert.Equal("ID is already in use", result.Message);
    }

    [Fact]
    public async Task UploadFileAsync_ReturnsOk_AndCallsCreate()
    {
        string id = "123";
        string data = "Data";
        var fileRequest = new FileUploadRequest { Id = id, Data = data };
        var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"localhost:9000/{id}");

        _mockDao.Setup(d => d.GetById(fileRequest.Id))!.ReturnsAsync((FileMetadata)null);
        _mockRequestProvider.Setup(d => d.Put(id, data)).Returns(httpRequest);
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", httpRequest, ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
            });

        var result = await _service.UploadFileAsync(fileRequest);

        Assert.True(result.Success);
        Assert.NotNull(result.Value);

        _mockDao.Verify(d => d.Create(It.Is<FileMetadata>(m => m.Id == id)), Times.Once);
    }
}