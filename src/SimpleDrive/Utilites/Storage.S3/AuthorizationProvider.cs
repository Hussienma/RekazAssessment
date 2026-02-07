using System.Text;
using SimpleDrive.Interfaces;

namespace SimpleDrive.Storage.S3.Utils;

public class AuthorizationProvider : IAuthorizationProvider
{
    private string _accessKeyId;
    private ISignatureProvider _signatureProvider;

    public AuthorizationProvider(IConfiguration config, ISignatureProvider signatureProvider)
    {
        _accessKeyId = config.GetValue<string>("S3:AccessKeyID")!;
        _signatureProvider = signatureProvider;
    }

    public string GetAuthorizationHeader(string method, string host, string path, DateTime timestampUTC, string payload = "", string region = "us-east-1", string service = "s3")
    {
        StringBuilder authHeaderBuilder = new StringBuilder("AWS4-HMAC-SHA256 ");

        Dictionary<string, string> headers = new Dictionary<string, string>();

        headers["host"] = host;
        // headers["Content-Length"] = payload.Length.ToString();
        headers["x-amz-date"] = timestampUTC.ToString("yyyyMMddTHHmmssZ");
        // request.Headers.Add("x-amz-content-sha256", );
        headers["x-amz-content-sha256"] = Encryption.SHA256Hash(payload);

        authHeaderBuilder.Append($"Credential={_accessKeyId}/{timestampUTC.ToString("yyyyMMdd")}/{region}/{service}/aws4_request,");
        authHeaderBuilder.Append($"SignedHeaders={string.Join(";", headers.Select(header => header.Key))},");

        string headersString = string.Join("\n", headers.Select(header => $"{header.Key}:{header.Value}"));

        string[] splitPath = path.Split("?");
        string fileUri = splitPath[0];
        string queries = "";

        if (splitPath.Length > 1) queries = splitPath[1];


        authHeaderBuilder.Append($"Signature={_signatureProvider.GetSignature(method, fileUri, queries, headersString, payload, region, service)}");

        return authHeaderBuilder.ToString();
    }

    public HttpRequestMessage CreateS3Request(
        string method,
        string host,
        string path,
        DateTime timestampUTC,
        string payload = "",
        string region = "us-east-1")
    {
        // 1. Initialize the Request
        // Note: 'path' should include the leading forward slash (e.g., "/my-bucket/file.txt")
        var requestUri = new Uri($"http://{host}{path}");
        var request = new HttpRequestMessage(new HttpMethod(method), requestUri);

        // 2. Add the exact headers used in your GetAuthorizationHeader function
        // The keys must match exactly what you put in your dictionary.
        string amzDate = timestampUTC.ToString("yyyyMMddTHHmmssZ");

        request.Headers.Host = host;
        request.Headers.Add("x-amz-date", amzDate);
        request.Headers.Add("x-amz-content-sha256", Encryption.SHA256Hash(payload));
        
        // request.Headers.Add("x-amz-content-sha256", "UNSIGNED-PAYLOAD");

        // 3. Generate the Authorization header using your provided function
        // We pass the same timestamp and parameters to ensure parity.
        string authHeader = GetAuthorizationHeader(
            method,
            host,
            path,
            timestampUTC,
            payload,
            region,
            "s3"
        );

        request.Headers.TryAddWithoutValidation("Authorization", authHeader);

        // 4. Handle Payload (if any)
        if (!string.IsNullOrEmpty(payload))
        {
            request.Content = new StringContent(payload, System.Text.Encoding.UTF8);
            // Note: Your Auth function doesn't currently sign 'Content-Type' or 'Content-Length'.
            // If you add them there, you must add them here as well.
        }

        if (method == "PUT" || method == "POST")
    {
        // Use ByteArrayContent to avoid default "text/plain" Content-Type if you don't want it
        var content = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(payload ?? ""));
        
        // IMPORTANT: If you didn't include Content-Type in your 'SignedHeaders', 
        // some S3 clones get grumpy if it's there. Let's keep it clean:
        content.Headers.ContentType = null; 
        
        request.Content = content;
    }

        return request;
    }
}

// create a function in C# that creates an HttpRequestMessage. This function returns a request that should be sent to S3 bucket. There is a function to generate the Authorization header that is provided later. Make sure both requests are identical so they generate the same signature.