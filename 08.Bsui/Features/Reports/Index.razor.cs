namespace Smlp.Bsui.Features.Reports;

public partial class Index
{
    private string _reportType = "transactions";
    private DateTime? _reportFrom = DateTime.Today.AddMonths(-1);
    private DateTime? _reportTo = DateTime.Today;
}
