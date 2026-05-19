using MudBlazor;

namespace Smlp.Bsui.Features.Analytics;

public partial class Index
{
    private DateRange _dateRange = new(DateTime.Today.AddDays(-30), DateTime.Today);
    private string _selectedPeriod = "30d";
}
