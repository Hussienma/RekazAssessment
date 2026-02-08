using System.Net.Http.Headers;
using System.Text;
using SimpleDrive.Interfaces;

namespace SimpleDrive.Storage.S3.Utils;

public class S3RequestProvider : IS3RequestProvider
{
    private string _accessKeyId;
    private string _host;
    private string _bucket;
    private string _region;

    private ISignatureProvider _signatureProvider;

    public S3RequestProvider(IConfiguration config, ISignatureProvider signatureProvider)
    {
        _accessKeyId = config.GetValue<string>("S3:AccessKeyID")!;
        _host = config.GetValue<string>("S3:Host")!;
        _bucket= config.GetValue<string>("S3:Bucket")!;
        _region = config.GetValue<string>("S3:Region")!;

        _signatureProvider = signatureProvider;
    }

    public string GetAuthorizationHeader(string method, string host, string path, DateTime timestampUTC, string payload = "", string region = "us-east-1", string service = "s3")
    {
        StringBuilder authHeaderBuilder = new StringBuilder("AWS4-HMAC-SHA256 ");

        Dictionary<string, string> headers = new Dictionary<string, string>();

        headers["host"] = host;
        headers["x-amz-date"] = timestampUTC.ToString("yyyyMMddTHHmmssZ");
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

    private HttpRequestMessage CreateRequest(
        string method,
        string fileId,
        string payload = "")
    {
        string path = $"/{_bucket}/{fileId}";
        var requestUri = new Uri($"http://{_host}{path}");
        var request = new HttpRequestMessage(new HttpMethod(method), requestUri);

        DateTime timestampUTC = DateTime.UtcNow;

        string amzDate = timestampUTC.ToString("yyyyMMddTHHmmssZ");

        request.Headers.Host = _host;
        request.Headers.Add("x-amz-date", amzDate);
        request.Headers.Add("x-amz-content-sha256", Encryption.SHA256Hash(payload));

        string authHeader = GetAuthorizationHeader(
            method,
            _host,
            path,
            timestampUTC,
            payload,
            _region,
            "s3"
        );

        request.Headers.TryAddWithoutValidation("Authorization", authHeader);

        if (!string.IsNullOrEmpty(payload) && method == "PUT" || method == "POST")
        {
            StringContent content = new StringContent(payload, Encoding.UTF8);
            request.Content = content;
        }

        return request;
    }

    public HttpRequestMessage Get(string path)
    {
        return CreateRequest("GET", path);
    }

    public HttpRequestMessage Put(string path, string payload)
    {
        return CreateRequest("PUT", path, payload);
    }

}
