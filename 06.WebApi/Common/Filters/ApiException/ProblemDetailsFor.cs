namespace Smlp.WebApi.Common.Filters.ApiException;

public static class ProblemDetailsFor
{
    public static class InvalidModelState
    {
        public const string Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
    }

    public static class ValidationException
    {
        public const string Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
    }

    public static class UnauthorizedAccessException
    {
        public const string Type = "https://tools.ietf.org/html/rfc7235#section-3.1";
        public const string Title = "You must be authorized to access this resource.";
    }

    public static class ForbiddenAccessException
    {
        public const string Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3";
        public const string Title = "You are forbidden to access this resource.";
    }

    public static class NotFoundException
    {
        public const string Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
        public const string Title = "The specified resource was not found.";
    }

    public static class AlreadyExistsExceptions
    {
        public const string Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8";
        public const string Title = "The specified resource was duplicate or conflict.";
    }

    public static class ArgumentException
    {
        public const string Type = "https://datatracker.ietf.org/doc/html/rfc4918#section-11.2";
        public const string Title = "One or more argument is not valid.";
    }

    public static class MismatchException
    {
        public const string Type = "https://datatracker.ietf.org/doc/html/rfc4918#section-11.2";
        public const string Title = "There is a mismatch between the value in the Route and the value in the Form.";
    }

    public static class InvalidOperationException
    {
        public const string Type = "https://datatracker.ietf.org/doc/html/rfc4918#section-11.2";
        public const string Title = "Cannot complete the operation because one or more related objects are not in proper state.";
    }

    public static class UnknownException
    {
        public const string Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
        public const string Title = "An error occurred while processing your request.";
    }

    //
}
