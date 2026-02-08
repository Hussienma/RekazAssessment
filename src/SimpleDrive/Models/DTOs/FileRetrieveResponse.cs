namespace SimpleDrive.DTOs;

public record GetFileResponse
{
    public string Id {get; set;}
    public int Size { get; set; }
    public string Data { get; set; }

    public DateTime CreateAt { get; set; }

}