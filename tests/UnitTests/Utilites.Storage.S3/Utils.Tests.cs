using SimpleDrive.Storage.S3.Utils;

public class UtilsTests
{
    #region Hex Tests
    [Theory]
    [InlineData("A", "559aead08264d5795d3909718cdd05abd49572e84fe55590eef31a88a08fdffd")]
    [InlineData("ABC", "b5d4045c3f466fa91fe2cc6abe79232a1a57cdf104f7a26e716e0a1e2789df78")]
    [InlineData("", "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855")]
    public void HexSHA256_String_ReturnsExpectedHex(string input, string expected)
    {
        var result = Encryption.SHA256Hash(input);

        Assert.Equal(expected, result);
    }
    #endregion
}