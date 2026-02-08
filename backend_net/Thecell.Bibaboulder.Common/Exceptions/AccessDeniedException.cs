namespace Thecell.Bibaboulder.Common.Exceptions;

/// <summary>
/// Access denied Exception.
/// </summary>
public class AccessDeniedException : System.Exception
{
    public AccessDeniedException()
    {
    }

    public AccessDeniedException(string message) : base(message)
    {
    }

    public AccessDeniedException(string message, System.Exception innerException) : base(message, innerException)
    {
    }
}
