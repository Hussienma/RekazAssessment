using System.Text;

namespace SimpleDrive.Storage.S3.Utils;

public class CanonicalRequestBuilder
{
    private string _method = "GET";
    private string _uri = "/";

    // Payload hash of the empty string if not hash is provided
    private string _payloadHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
    private Dictionary<string, string> _headers = new();
    private Dictionary<string, string> _queryParams = new();

    public CanonicalRequestBuilder WithMethod(string method)
    {
        _method = method.ToUpperInvariant();
        return this;
    }

private static string UriEncode(string str, bool encodeSlash = true)
    {
        StringBuilder sb = new StringBuilder();

        List<char> allowedChars = Enumerable.Range('A', 26) // A-Z
            .Concat(Enumerable.Range('a', 26)) // a-z
            .Concat(Enumerable.Range('0', 10)) // 0-9
            .Select(i => (char)i)
            .Concat(new[] { '-', '.', '_', '~', }) // Special characters
            .ToList();

        if (!encodeSlash) allowedChars.Add('/');

        foreach (char c in str)
        {
            if (allowedChars.Contains(c))
            {
                sb.Append(c);
            }
            else
            {
                sb.Append($"%{((int)c).ToString("X")}");
            }
        }

        return sb.ToString();
    }

    public CanonicalRequestBuilder WithUri(string uri)
    {
        _uri = UriEncode(uri, false);
        return this;
    }

    public CanonicalRequestBuilder WithQueries(string queries)
    {
        _queryParams = queries.Split("&", StringSplitOptions.RemoveEmptyEntries).Select(query => query.Split("=")).ToDictionary(query => UriEncode(query[0].Trim()), query => UriEncode(query.Length > 1?query[1] : "").Trim());
        return this;
    }

    public CanonicalRequestBuilder WithHeaders(string headers)
    {
        _headers= 
            headers.Split("\n", StringSplitOptions.RemoveEmptyEntries)
                .Select(header => header.Split(":", 2))
                .ToDictionary(header => header[0].ToLowerInvariant().Trim(), header => header[1].Trim());
        return this;
    }

    public CanonicalRequestBuilder WithPayload(string payload)
    {
        _payloadHash = Encryption.SHA256Hash(payload);
        return this;
    }

    public string Build()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append($"{_method}\n");
        sb.Append($"{_uri}\n");

        var query = string.Join("&", _queryParams.Select(p => $"{p.Key}={p.Value}"));
        sb.Append(query);

        sb.Append('\n');

        Dictionary<string, string> sortedHeaders = _headers.OrderBy(h => h.Key).ToDictionary();
        foreach (var header in sortedHeaders)
        {
            sb.Append($"{header.Key}:{header.Value}\n");
        }

        sb.Append('\n');

        string signedHeaders = string.Join(";", sortedHeaders.Select(h => h.Key));

        sb.Append($"{signedHeaders}\n");

        sb.Append(_payloadHash);

        return sb.ToString();
    }
}
