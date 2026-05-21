namespace ComputerStore.Application.Common;

public class AppException : Exception
{
    public AppException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}

public sealed class NotFoundAppException : AppException
{
    public NotFoundAppException(string message) : base(message, 404)
    {
    }
}

public sealed class StockAppException : AppException
{
    public StockAppException(string message) : base(message, 409)
    {
    }
}
