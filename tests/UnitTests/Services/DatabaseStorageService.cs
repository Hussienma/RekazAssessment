using Moq;
using SimpleDrive.Interfaces;
using SimpleDrive.Services;
using SimpleDrive.Entities;
using SimpleDrive.DTOs;

namespace UnitTests.Services;

public class DatabaseStorageServiceTests
{
    private readonly Mock<IFileRecordDao> _mockRecordDao;
    private readonly Mock<IFileMetadataDao> _mockMetadataDao;
    private readonly DatabaseStorageService _service;

    public DatabaseStorageServiceTests()
    {
        _mockRecordDao = new Mock<IFileRecordDao>();
        _mockMetadataDao = new Mock<IFileMetadataDao>();
        
        _service = new DatabaseStorageService(_mockRecordDao.Object, _mockMetadataDao.Object);
    }

    [Fact]
    public async Task GetFileById_ReturnsFailure_WhenMetadataDoesNotExist()
    {
        _mockMetadataDao.Setup(d => d.GetById("123"))!.ReturnsAsync((FileMetadata)null);

        var result = await _service.GetFileById("123");

        Assert.False(result.Success);
        Assert.Equal("File not found", result.Message);
    }

    [Fact]
    public async Task GetFileById_ReturnsFailure_WhenRecordDataIsMissing()
    {
        var metadata = new FileMetadata { Id = "123" };
        _mockMetadataDao.Setup(d => d.GetById("123")).ReturnsAsync(metadata);
        _mockRecordDao.Setup(d => d.GetById("123"))!.ReturnsAsync((FileRecord)null);

        var result = await _service.GetFileById("123");

        Assert.False(result.Success);
        Assert.Equal("File data not found in storage", result.Message);
    }

    [Fact]
    public async Task GetFileById_ReturnsOk_WhenBothExist()
    {
        var id = "123";
        var metadata = new FileMetadata { Id = id, Size = 100 };
        var record = new FileRecord { Id = id, Data = "Data"};

        _mockMetadataDao.Setup(d => d.GetById(id)).ReturnsAsync(metadata);
        _mockRecordDao.Setup(d => d.GetById(id)).ReturnsAsync(record);

        var result = await _service.GetFileById(id);

        Assert.True(result.Success);
        Assert.Equal(id, result.Value!.Id);
        Assert.Equal(record.Data, result.Value.Data);
    }

    [Fact]
    public async Task UploadFileAsync_ReturnsFailure_WhenIdAlreadyExists()
    {
        var request = new FileUploadRequest { Id = "existing-id", Data = "Data" };
        _mockMetadataDao.Setup(d => d.GetById(request.Id)).ReturnsAsync(new FileMetadata());

        var result = await _service.UploadFileAsync(request);

        Assert.False(result.Success);
        Assert.Equal("ID is already in use", result.Message);
        _mockRecordDao.Verify(d => d.Create(It.IsAny<FileRecord>()), Times.Never);
    }

    [Fact]
    public async Task UploadFileAsync_ReturnsOk_AndCallsCreateOnBothDaos()
    {
        var request = new FileUploadRequest { Id = "new-id", Data = "Data" };
        _mockMetadataDao.Setup(d => d.GetById(request.Id))!.ReturnsAsync((FileMetadata)null);

        var result = await _service.UploadFileAsync(request);

        Assert.True(result.Success);
        Assert.Equal(4, result.Value!.Size);
        
        _mockRecordDao.Verify(d => d.Create(It.Is<FileRecord>(r => r.Id == "new-id")), Times.Once);
        _mockMetadataDao.Verify(d => d.Create(It.Is<FileMetadata>(m => m.Id == "new-id")), Times.Once);
    }
}