namespace SimpleDrive.DTOs;

public class ServiceResult {
    public bool Success { get; set; }
    public string Message { get; set; }
    public static ServiceResult Ok() => new ServiceResult { Success = true };
    public static ServiceResult Failure(string msg) => new ServiceResult { Success = false, Message = msg };
}