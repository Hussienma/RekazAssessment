using Moq;
using SimpleDrive.Interfaces;
using SimpleDrive.Services;
using SimpleDrive.Entities;
using SimpleDrive.DTOs;
using Microsoft.Extensions.Configuration;

namespace UnitTests.Services;

public class LocalStorageServiceTests
{
    private readonly Mock<IFileMetadataDao> _mockDao;
    private readonly Mock<IFileSystem> _mockFileSystem;
    private readonly LocalStorageService _service;

    public LocalStorageServiceTests()
    {
        _mockDao = new Mock<IFileMetadataDao>();
        _mockFileSystem = new Mock<IFileSystem>();

        var inMemorySettings = new Dictionary<string, string?> {
            {"StorageSettings:Local:DirectoryPath", "/home"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        
        _service = new LocalStorageService(configuration, _mockFileSystem.Object, _mockDao.Object);
    }

    [Fact]
    public async Task GetFileById_ReturnsFailure_WhenMetadataDoesNotExist()
    {
        _mockDao.Setup(d => d.GetById("123"))!.ReturnsAsync((FileMetadata)null);
        _mockFileSystem.Setup(d => d.FileExists("/home/123")).Returns(false);

        var result = await _service.GetFileById("123");

        Assert.False(result.Success);
        Assert.Equal("File not found", result.Message);

        _mockDao.Verify(d => d.GetById(It.IsAny<string>()), Times.Once);
        _mockFileSystem.Verify(d => d.FileExists(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetFileById_ReturnsFailure_WhenFileDoesNotExist()
    {
        var metadata = new FileMetadata { Id = "123" };
        _mockDao.Setup(d => d.GetById("123")).ReturnsAsync(metadata);
        _mockFileSystem.Setup(d => d.FileExists("/home/123")).Returns(false);

        var result = await _service.GetFileById("123");

        Assert.False(result.Success);
        Assert.Equal("File not found in storage", result.Message);

        _mockDao.Verify(d => d.GetById(It.IsAny<string>()), Times.Once);
        _mockFileSystem.Verify(d => d.FileExists(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetFileById_ReturnsOk_WhenBothExist()
    {
        string id = "123";
        string fileContent = "Content";
        int fileSize = 100;
        DateTime createAt = DateTime.UtcNow;

        var metadata = new FileMetadata { Id = id, Size = fileSize, CreatedAt = createAt };

        _mockDao.Setup(d => d.GetById(id)).ReturnsAsync(metadata);
        _mockFileSystem.Setup(d => d.FileExists("/home/123")).Returns(true);
        _mockFileSystem.Setup(d => d.ReadAllText("/home/123")).Returns(fileContent);

        var result = await _service.GetFileById(id);

        Assert.True(result.Success);
        Assert.Equal(id, result.Value!.Id);
        Assert.Equal(fileSize, result.Value!.Size);
        Assert.Equal(createAt, result.Value!.Create_At);
        Assert.Equal(fileContent, result.Value!.Data);
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
        var request = new FileUploadRequest { Id = id, Data = data };
        _mockDao.Setup(d => d.GetById(request.Id))!.ReturnsAsync((FileMetadata)null);
        _mockFileSystem.Setup(d => d.DirectoryExists(It.IsAny<string>())).Returns(false);
        _mockFileSystem.Setup(d => d.CreateDirectory(It.IsAny<string>()));

        var result = await _service.UploadFileAsync(request);

        Assert.True(result.Success);
        Assert.Equal(request.Data.Length, result.Value!.Size);
        
        _mockDao.Verify(d => d.Create(It.Is<FileMetadata>(m => m.Id == id)), Times.Once);

        _mockFileSystem.Verify(d => d.CreateDirectory(It.IsAny<string>()), Times.Once);
        _mockFileSystem.Verify(d => d.WriteAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}