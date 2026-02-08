using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using SimpleDrive.Interfaces;

namespace SimpleDrive.Storage.S3.Utils;

public class SignatureProvider : ISignatureProvider
{
    private string _accessSecret;

    public SignatureProvider(IConfiguration config)
    {
        _accessSecret = config.GetValue<string>("StorageSettings:S3:AccessKeySecret")!;
    }

    private string GetStringToSign(string canonicalRequest, string timestampISO8601, string region, string service)
    {
        StringBuilder stringBuilder = new StringBuilder("AWS4-HMAC-SHA256\n");

        stringBuilder.Append($"{timestampISO8601}\n"); // timestamp to ISO8601 (with no spaces)

        string scope = $"{timestampISO8601.Substring(0, 8)}/{region}/{service}/aws4_request";
        stringBuilder.Append($"{scope}\n");

        stringBuilder.Append($"{Encryption.SHA256Hash(canonicalRequest)}");

        return stringBuilder.ToString();
    }

    private byte[] GetSigningString(string timestampISO8601, string region, string service)
    {
        byte[] awsKey = Encoding.Default.GetBytes($"AWS4{_accessSecret}");

        byte[] dateKey = Encryption.HMAC_SHA256(awsKey, timestampISO8601.Substring(0, 8));
        byte[] dateRegionKey = Encryption.HMAC_SHA256(dateKey, region);
        byte[] dateRegionServiceKey = Encryption.HMAC_SHA256(dateRegionKey, service);
        byte[] signingKey = Encryption.HMAC_SHA256(dateRegionServiceKey, "aws4_request");

        return signingKey;
    }

    public string GetSignature(string method, string path, string queries, string headers, string payload, string region, string service)
    {
        // HERE the payload should either be signed and included in x-amz-sha256-hash or should not be included in the canonical request
        CanonicalRequestBuilder canonicalRequestBuilder = new CanonicalRequestBuilder().WithMethod(method).WithUri(path).WithPayload(payload).WithHeaders(headers).WithQueries(queries);

        string canonicalRequest = canonicalRequestBuilder.Build();

        Match amzDateMatch = Regex.Match(headers, @"(?<=x-amz-date:\s*).*");
        if (!amzDateMatch.Success)
            throw new DataException("x-amz-date header is required");

        string amzDate = amzDateMatch.Value.Trim();

        string stringToSign = GetStringToSign(canonicalRequest, amzDate, region, service);
        byte[] signingKey = GetSigningString(amzDate, region, service);
        byte[] signature = Encryption.HMAC_SHA256(signingKey, stringToSign);

        return Convert.ToHexString(signature).ToLowerInvariant();
    }
}