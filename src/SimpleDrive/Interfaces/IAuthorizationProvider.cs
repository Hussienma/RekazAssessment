namespace SimpleDrive.Interfaces;

public interface IAuthorizationProvider
{
    string GetAuthorizationHeader(string method, string host, string path, DateTime timestamp, string payload = "", string region = "us-east-1", string service = "s3");
    public HttpRequestMessage CreateS3Request(
        string method,
        string host,
        string path,
        DateTime timestampUTC,
        string payload = "",
        string region = "us-east-1");
}