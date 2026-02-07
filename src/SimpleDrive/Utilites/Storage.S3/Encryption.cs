using System.Security.Cryptography;
using System.Text;

namespace SimpleDrive.Storage.S3.Utils;

public static class Encryption
{
    public static string SHA256Hash(string data)
    {
        byte[] dataBytes = Encoding.Default.GetBytes(data);

        byte[] hash = SHA256.HashData(dataBytes);

        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static byte[] HMAC_SHA256(byte[] signingKey, string stringToSign)
    {
        byte[] dataBytes = Encoding.UTF8.GetBytes(stringToSign);

        return HMACSHA256.HashData(signingKey, dataBytes);
    }
}