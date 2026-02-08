namespace SimpleDrive.DTOs;

public record FileUploadRequest
{
    public string Id { get; set; }
    public string Data { get; set; }
}