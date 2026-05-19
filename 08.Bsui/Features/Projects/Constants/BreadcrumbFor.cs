using MudBlazor;
using Psmp.Shared.Projects.Constants;

namespace Psmp.Bsui.Features.Projects.Constants;

public static class BreadcrumbFor
{
    public static readonly BreadcrumbItem Index = new(DisplayTextFor.Projects, href: RouteFor.EditProjectProposal);
}
