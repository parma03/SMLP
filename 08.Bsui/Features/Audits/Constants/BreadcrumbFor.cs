using MudBlazor;
using Psmp.Shared.Audits.Constants;

namespace Psmp.Bsui.Features.Audits.Constants;

public static class BreadcrumbFor
{
    public static readonly BreadcrumbItem Index = new(DisplayTextFor.Audits, href: RouteFor.Index);
}
