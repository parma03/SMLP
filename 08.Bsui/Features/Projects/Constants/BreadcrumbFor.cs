using MudBlazor;
using Smlp.Shared.Projects.Constants;

namespace Smlp.Bsui.Features.Projects.Constants;

public static class BreadcrumbFor
{
    public static readonly BreadcrumbItem Index = new(DisplayTextFor.Projects, href: RouteFor.EditProjectProposal);
}
