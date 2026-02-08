namespace SimpleDrive.Common;

public class Result<T> {
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Value {get; set;}

    private Result(bool success, T? value, string message) 
    {
        Success = success;
        Value = value;
        Message = message;
    }

    public static Result<T> Ok(T? value) => new (true, value, "");
    public static Result<T> Failure(string msg) => new ( false, default, msg);

}