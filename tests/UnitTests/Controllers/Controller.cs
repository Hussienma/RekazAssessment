using Moq;
using Microsoft.AspNetCore.Mvc;
using SimpleDrive.Interfaces;
using SimpleDrive.DTOs;
using SimpleDrive.Common;
using SimpleDrive.Entities;
using Microsoft.Extensions.Configuration;

public class BlobControllerTests
{
    private readonly Mock<IStorageService> _mockStorageService;
    private readonly SimpleDrive.Controllers.Controller _controller;

    public BlobControllerTests()
    {
        _mockStorageService = new Mock<IStorageService>();
        var inMemorySettings = new Dictionary<string, string?> {
            {"Jwt:Key", "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY"}, {"Jwt:Issuer", "Issuer"}, {"Jwt:Audience", "Audience"},
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _controller = new SimpleDrive.Controllers.Controller(_mockStorageService.Object, configuration);
    }

    [Fact]
    public async Task UploadFileAsync_ReturnsOk_WhenUploadIsSuccessful()
    {
        var request = new FileUploadRequest { Id = "123", Data = "SGVsbG8gU2ltcGxlIFN0b3JhZ2UgV29ybGQh" };
        var serviceResponse = Result<FileMetadata>.Ok(new FileMetadata());

        _mockStorageService.Setup(s => s.UploadFileAsync(request)).ReturnsAsync(serviceResponse);

        var result = await _controller.UploadFileAsync(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task UploadFileAsync_ReturnsBadRequest_WhenUploadFails()
    {
        var request = new FileUploadRequest();
        var serviceResponse = Result<FileMetadata>.Failure("File not found");

        _mockStorageService.Setup(s => s.UploadFileAsync(request))
            .ReturnsAsync(serviceResponse);

        var result = await _controller.UploadFileAsync(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("File not found", badRequestResult.Value);
    }

    [Fact]
    public async Task GetFileById_ReturnsOk_WhenFileExists()
    {
        string fileId = "file.txt";
        FileGetResponse fileGetResponse = new FileGetResponse { Id = fileId };
        var serviceResponse = Result<FileGetResponse>.Ok(fileGetResponse);

        _mockStorageService.Setup(s => s.GetFileById(fileId))
            .ReturnsAsync(serviceResponse);

        var result = await _controller.GetFileById(fileId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(okResult.Value, fileGetResponse);
    }

    [Fact]
    public async Task GetFileById_ReturnsNotFound_WhenFileDoesNotExist()
    {
        string fileId = "missing-id";
        string errorMsg = "File not found";
        var serviceResponse = Result<FileGetResponse>.Failure(errorMsg);

        _mockStorageService.Setup(s => s.GetFileById(fileId))
            .ReturnsAsync(serviceResponse);

        var result = await _controller.GetFileById(fileId);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(errorMsg, notFoundResult.Value);
    }
}