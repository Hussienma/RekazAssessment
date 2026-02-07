using SimpleDrive.Storage.S3.Utils;
using SimpleDrive.Interfaces;
using Moq;
using Microsoft.Extensions.Configuration;

namespace UnitTests;

public class StorageTests
{
    [Fact]
    public void Build_CanonicalRequestBuilder_ReturnsExpectedFormat()
    {
        string canonicalRequest = new CanonicalRequestBuilder()
            .WithMethod("GET").WithUri("/test.txt")
            .WithHeaders("Host: examplebucket.s3.amazonaws.com\nRange: bytes=0-9\nx-amz-content-sha256:e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\nx-amz-date: 20130524T000000Z\n")
            .Build();

        Assert.Equal("GET\n/test.txt\n\nhost:examplebucket.s3.amazonaws.com\nrange:bytes=0-9\nx-amz-content-sha256:e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\nx-amz-date:20130524T000000Z\n\nhost;range;x-amz-content-sha256;x-amz-date\ne3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
         canonicalRequest);
    }

    // The values of the tests and the expected output are taken from https://docs.aws.amazon.com/AmazonS3/latest/API/sig-v4-header-based-auth.html
    [Theory]
    [InlineData("GET", "/test.txt", "", "Host: examplebucket.s3.amazonaws.com\nRange: bytes=0-9\nx-amz-content-sha256:e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\nx-amz-date: 20130524T000000Z\n", "", "us-east-1", "s3", "f0e8bdb87c964420e857bd35b5d6ed310bd44f0170aba48dd91039c6036bdb41")]
    [InlineData("PUT", "/test$file.text", "", "Host: examplebucket.s3.amazonaws.com\nDate: Fri, 24 May 2013 00:00:00 GMT\nx-amz-date: 20130524T000000Z \nx-amz-storage-class: REDUCED_REDUNDANCY\nx-amz-content-sha256: 44ce7dd67c959e0d3524ffac1771dfbba87d2b6b4b4e99e42034a8b803f8b072\n", "Welcome to Amazon S3.", "us-east-1", "s3", "98ad721746da40c64f1a55b78f14c238d841ea1380cd77a1b5971af0ece108bd")]
    [InlineData("GET", "/", "lifecycle", "Host: examplebucket.s3.amazonaws.com\nx-amz-date: 20130524T000000Z\nx-amz-content-sha256:e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\n", "", "us-east-1", "s3", "fea454ca298b7da1c68078a5d1bdbfbbe0d65c699e0f91ac7a200a0136783543")]
    [InlineData("GET", "/", "max-keys=2&prefix=J", "Host: examplebucket.s3.amazonaws.com\nx-amz-date: 20130524T000000Z\nx-amz-content-sha256:e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855\n", "", "us-east-1", "s3", "34b48302e7b5fa45bde8084f4b7868a86f0a534bc59db6670ed5711ef69dc6f7")]
    public void SignatureProvider_GetSignature_ReturnsExpectedSignature(string method, string path, string queries, string headers, string payload, string region, string service, string expected)
    {
        var inMemorySettings = new Dictionary<string, string> {
    {"S3:AccessKeySecret", "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY"},
};

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        SignatureProvider signatureProvider = new SignatureProvider(configuration);

        string signature = signatureProvider.GetSignature(method, path, queries, headers, payload, region, service);
        Assert.Equal(expected, signature);
    }

    [Theory]
    [InlineData("GET", "s3.amazonaws.com", "/mybucket/file.txt", "us-east-1", "s3")]
    [InlineData("POST", "dynamodb.us-west-2.amazonaws.com", "/", "us-west-2", "dynamodb")]
    public void GetAuthorizationHeader_ReturnsValidFormat(string method, string host, string path, string region, string service)
    {
        var mockSignatureProvider = new Mock<ISignatureProvider>();
        string dummySignature = "mocked_signature_12345";

        mockSignatureProvider
            .Setup(s => s.GetSignature(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(dummySignature);

        var inMemorySettings = new Dictionary<string, string> {
    {"S3:AccessKeyID", "access-key-id"},
};

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var serviceClient = new AuthorizationProvider(configuration, mockSignatureProvider.Object);
        DateTime utcNow = DateTime.UtcNow;
        string date = utcNow.ToString("yyyyMMdd");

        var result = serviceClient.GetAuthorizationHeader(method, host, path, utcNow, region: region, service: service);

        Assert.StartsWith("AWS4-HMAC-SHA256 ", result);
        Assert.Contains($"Credential=access-key-id/{date}/{region}/{service}/aws4_request", result);
        Assert.Contains("SignedHeaders=", result);
        Assert.EndsWith($"Signature={dummySignature}", result);
    }
}
