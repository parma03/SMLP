namespace Smlp.Bsui.Features.Scholarships.ScholarshipPlan.Constants;

public static class RouteFor
{
    public static readonly string Scholarships = $"{nameof(Scholarships)}";
    public static readonly string Index = $"{Scholarships}/{nameof(ScholarshipPlan)}";
    public static readonly string Register = $"{Scholarships}/{nameof(ScholarshipPlan)}/{nameof(Registration)}";

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

    public static string Draft()
    {
        return $"{Index}/{nameof(Draft)}";
    }
}
