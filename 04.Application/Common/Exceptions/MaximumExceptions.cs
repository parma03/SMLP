namespace Smlp.Application.Common.Exceptions;

public class MaximumException : Exception
{
    public MaximumException()
        : base()
    {
    }

    public MaximumException(string message)
        : base(message)
    {
    }
}
