using MudBlazor;
using Smlp.Shared.Audits.Constants;

namespace Smlp.Bsui.Features.Audits.Constants;

public static class BreadcrumbFor
{
    public static readonly BreadcrumbItem Index = new(DisplayTextFor.Audits, href: RouteFor.Index);
}
