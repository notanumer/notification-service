namespace BaseMicroservice;

public class SendMessageResult
{
    public SendMessageResult(bool isSuccess, Exception? exception)
    {
        IsSuccess = isSuccess;
        Exception = exception;
    }

    public static SendMessageResult Success() => new SendMessageResult(true, null);

    public static SendMessageResult Fail(Exception exception) => new SendMessageResult(false, exception);

    public Exception? Exception { get; }

    public bool IsSuccess { get; }
}