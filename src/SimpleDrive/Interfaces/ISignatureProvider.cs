namespace SimpleDrive.Interfaces;

public interface ISignatureProvider
{
    public string GetSignature(string method, string path, string queries, string headers, string payload, string region, string service);
}