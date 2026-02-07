namespace SimpleDrive.DTOs;

public record FileUploadRequest
{
    public string id { get; set; }
    public string data { get; set; }
}