using SimpleDrive.Common;
using SimpleDrive.DTOs;
using SimpleDrive.Entities;

namespace SimpleDrive.Utils;

public class Validation
{
    public static bool IsBase64String(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64)) return false;

        var base64Data = base64.Contains(",") ? base64.Split(',')[1] : base64;

        Span<byte> buffer = new Span<byte>(new byte[base64Data.Length]);
        return Convert.TryFromBase64String(base64Data, buffer, out _);
    }

    public static Result<FileMetadata> ValidateUploadRequest(FileUploadRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Id)) return Result<FileMetadata>.Failure("ID is required");
        if (!IsBase64String(request.Data)) return Result<FileMetadata>.Failure("Data could not be decoded");

        return Result<FileMetadata>.Ok(null);
    }
}