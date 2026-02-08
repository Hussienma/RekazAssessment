namespace SimpleDrive.Interfaces;

public interface IS3RequestProvider
{
    string GetAuthorizationHeader(string method, string host, string path, DateTime timestamp, string payload = "", string region = "us-east-1", string service = "s3");
            HttpRequestMessage Get(string path);
        HttpRequestMessage Put(string path, string payload);
}