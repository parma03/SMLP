namespace Psmp.Bsui.Features.StudyPreparations.SponsorshipLetter.Components;

public partial class DraftItemData
{

    #region Tooltip
    protected string? _contentTooltipPlaceDate = "";
    protected string? _contentTooltipNummberOfLetter = "";
    protected string? _contentTooltipInvoiceAddressPosition = "";
    protected string? _contentTooltipInvoiceAddressName = "";
    protected string? _contentTooltipInvoiceAddressCompany = "";
    protected string? _contentTooltipMainPicPosition = "";
    protected string? _contentTooltipMainPicName = "";
    protected string? _contentTooltipMainPicEmail = "";
    protected string? _contentTooltipCarbonCopy = "";
    #endregion

    //protected override async Task OnInitializedAsync()
    //{
    //    await OnLoadTooltip();
    //    await Task.CompletedTask;
    //}

    private async Task OnLoadTooltip()
    {
        _contentTooltipPlaceDate = @"Use tag {{PlaceDate}} and add to document. If use multiple lease use ; and use tag {{PlaceDate0}},...,{{PlaceDateN}} same split data";
        _contentTooltipNummberOfLetter = @"Use tag {{NumberOfLetter}} and add to document. If use multiple lease use ; and use tag {{NumberOfLetter0}},...,{{NumberOfLetterN}} same split data";
        _contentTooltipInvoiceAddressPosition = @"Use tag {{InvoiceAddressPosition}} and add to document. If use multiple lease use ; and use tag {{InvoiceAddressPosition0}},...,{{InvoiceAddressPositionN}} same split data";
        _contentTooltipInvoiceAddressName = @"Use tag {{InvoiceAddressName}} and add to document. If use multiple lease use ; and use tag {{InvoiceAddressName0}},...,{{InvoiceAddressNameN}} same split data";
        _contentTooltipInvoiceAddressCompany = @"Use tag {{InvoiceAddressCompany}} and add to document. If use multiple lease use ; and use tag {{InvoiceAddressCompany0}},...,{{InvoiceAddressCompanyN}} same split data";
        _contentTooltipMainPicPosition = @"Use tag {{MainPicPosition}} and add to document. If use multiple lease use ; and use tag {{MainPicPosition0}},...,{{MainPicPositionN}} same split data";
        _contentTooltipMainPicName = @"Use tag {{MainPicName}} and add to document. If use multiple lease use ; and use tag {{MainPicName0}},...,{{MainPicNameN}} same split data";
        _contentTooltipMainPicEmail = @"Use tag {{MainPicEmail}} and add to document. If use multiple lease use ; and use tag {{MainPicEmail0}},...,{{MainPicEmailN}} same split data";
        _contentTooltipCarbonCopy = @"Use tag {{CarbonCopy}} and add to document. If use multiple lease use ; and use tag {{CarbonCopy0}},...,{{CarbonCopyN}} same split data";

        await Task.CompletedTask;
    }
}
