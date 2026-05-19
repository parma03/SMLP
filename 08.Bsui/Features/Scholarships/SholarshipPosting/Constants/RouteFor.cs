namespace Smlp.Bsui.Features.Scholarships.SholarshipPosting.Constants;

public static class RouteFor
{
    public static readonly string Scholarships = $"{nameof(Scholarships)}";
    public static readonly string Index = $"{nameof(Scholarships)}/{nameof(SholarshipPosting)}";
    public static readonly string Register = $"{nameof(Scholarships)}/{nameof(SholarshipPosting)}/{nameof(Registration)}";

    public static string Registration(bool isCreated, Guid id)
    {
        return $"{Register}/{isCreated}/{id}";
    }

    public static string Created(bool isCreated)
    {
        return $"{Register}/{isCreated}";
    }
    public static string Deleted(Guid id)
    {
        return $"{Index}/Deletion/{id}";
    }
}
