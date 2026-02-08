namespace Thecell.Bibaboulder.Common.Exceptions;

public class AuthenticationException : System.Exception
{
    public AuthenticationException()
    {
    }

    public AuthenticationException(string message) : base(message)
    {
    }

    public AuthenticationException(string message, System.Exception innerException) : base(message, innerException)
    {
    }
}
