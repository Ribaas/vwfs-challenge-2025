namespace Frete.Domain.Exceptions;

public class InvalidFreteParametrosException : DomainException
{
    public InvalidFreteParametrosException(string message)
        : base(message)
    {
    }
}
