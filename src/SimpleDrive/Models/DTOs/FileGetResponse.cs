namespace SimpleDrive.DTOs;

public record FileGetResponse
{
    public string Id {get; set;}
    public int Size { get; set; }
    public string Data { get; set; }

    public DateTime Create_At { get; set; }

}