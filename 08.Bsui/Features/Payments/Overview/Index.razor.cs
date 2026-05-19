using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Payments.Queries;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using System.Security.Claims;
using Psmp.Base.Common;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Payments.Commands;
using Psmp.Bsui.Common.Extensions;
using System.Data;
using Psmp.Base.Payments.Enums;
using Psmp.Base.Scholarships.Enums;
using Psmp.Shared.Services.Authorization.Constants;
using Syncfusion.Blazor.Navigations;
using Psmp.Bsui.Features.Payments.Constants;

namespace Psmp.Bsui.Features.Payments.Overview;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    protected List<GetPaymentOverviewResponse> _paymentOverview = new();
    private SfGrid<GetPaymentOverviewResponse> Grid { get; set; } = new();

    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;

    public List<DropdownItemResponse> _dropdownCategories = new();
    public List<DropdownItemResponse> _dropdownHps = new();
    public List<DropdownItemStringResponse> _dropdownCompany = new();
    public List<DropdownItemResponse> _dropdownDirectorate = new();
    public List<DropdownItemResponse> _dropdownScholarshipCode = new();
    public List<DropdownItemResponse> _dropdownBatch = new();
    private List<DropdownItemResponse> _dropdownItemCurrencyType = new();

    public List<GetPaymentOverviewDetailResponse> _paymentOverviewDetail = new();
    public PaymentOverviewRequest _paymentOverviewRequest = new();
    private string TitlePopup { get; set; } = "Request Overview";
    private bool VisiblePopup { get; set; }
    private bool IsEnabled { get; set; }

    private bool VisiblePopupDelete { get; set; } = false;

    private decimal _countcurrency1;
    private decimal _countcurrency2;
    private decimal _countcurrency3;
    private decimal _countcurrency4;

    private decimal _countcurrency5;
    private decimal _countcurrency6;
    private decimal _countcurrency7;
    private decimal _countcurrency8;

    private decimal _countcurrency9;
    private decimal _countcurrency10;

    private int _count1;
    private int _count2;
    private int _count3;
    private int _count4;

    private int _count5;
    private int _count6;
    private int _count7;
    private int _count8;

    private int _count9;
    private int _count10;
    protected long _maximumFileSizeInBytes = long.MaxValue;
    protected SfUploader? _uploadObj;

    public string? _requestID;
    public string? _wageType;
    public decimal? _ammount;
    public string? _curency;
    public string? _assignmentNumber;
    public string? _costCenter;
    public DateOnly? _paymentDate;
    public PaymentStatus _paymentStatus;
    protected string _employeeId = string.Empty;
    protected string _username = string.Empty;
    protected int _companyGroupId = 0;
    protected string _companyCode = string.Empty;
    protected int _directorateId = 0;
    protected int _category = 0;

    protected DateOnly? _currentPaymentDate;
    public bool IsAllowApprovePayment { get; set; } = false;
    protected override async Task OnInitializedAsync()
    {
        _dropdownItemCurrencyType = EnumExtentions.ToDropdownItem<CurrencyType>();
        _user = (await AuthenticationStateTask).User;
        if (_user.Identity.IsAuthenticated)
        {

            if (_user.Claims.Where(x => x.Type.ToLower().Trim() == "permission" && x.Value.ToLower().Trim().Contains(Permissions.Psmp_Payment_Approve)).ToList().Count > 0)
            {
                IsAllowApprovePayment = true;
            }

            _companyCode = _user.GetCompanyCode();
            _isLoading = true;
            await SetDataOnLoadAsync();
            await GetDropdownCategories();
            await GetDropdownCompanyGroup();
            await GetDropdownCompany();
            await GetDropdownDirectorate();
            _isLoading = false;
        }
    }

    #region FIltering Dropdown
    private async Task GetDropdownCompanyGroup()
    {
        var data = await _companyGroupService.GetCompanyGroupsAsync();
        if (data.Error is null)
        {
            _dropdownHps = data.Result!.Items.Where(x => _companyCode == "1010" ? true : x.CompanyCode == _companyCode).Select(x => new DropdownItemResponse { Id = x.Id, Description = x.Name }).ToList();
        }
    }

    private async Task GetDropdownCompany()
    {
        var data = await _companyService.GetCompanyByCompanyGroupIdAsync(_companyGroupId);
        if (data.Error is null)
        {
            _dropdownCompany = data.Result!.Items.Select(x => new DropdownItemStringResponse { Id = x.Code, Description = x.Name }).OrderBy(x => x.Id).ToList();
        }
    }
    private async Task GetDropdownDirectorate()
    {
        var data = await _directorateService.GetDirectotaeByCompanyCodeAsync(_companyCode);
        if (data.Error is null)
        {
            _dropdownDirectorate = data.Result!.Items.Select(x => new DropdownItemResponse { Id = x.Id, Description = x.Name }).OrderBy(x => x.Id).ToList();
        }
    }
    private async Task GetDropdownCategories()
    {
        _dropdownCategories = EnumExtentions.ToDropdownItem<ScholarshipLocation>();
    }

    public async Task OnChangeCompanyGroupAsync()
    {
        await GetDropdownCompany();
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    public async Task OnChangeCompanyAsync()
    {
        await GetDropdownDirectorate();
        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    public async Task OnChangeDirectorateAsync()
    {
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    public async Task OnChangeCategory()
    {
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    #endregion

    private async Task SetDataOnLoadAsync()
    {
        _isLoadingData = true;
        await Task.Delay(2000);
        var request = new GetPaymentOverviewRequest()
        {
            CompanyGroup = _companyGroupId,
            Company = _companyCode,
            Category = _category,
            Directorate = _directorateId
        };
        var response = await _paymentService.GetPaymentOverviewAsync(request);
        if (response.Error is null)
        {
            _paymentOverview = response.Result!.Items.ToList();
            _currentPaymentDate = _currentPaymentDate = _paymentOverview.Count() > 0 ? _paymentOverview!.OrderByDescending(x => x.PaymentDate).FirstOrDefault()!.PaymentDate != null ? _paymentOverview!.OrderByDescending(x => x.PaymentDate).FirstOrDefault()!.PaymentDate : DateOnly.FromDateTime(DateTime.Now) : DateOnly.FromDateTime(DateTime.Now);
            _countcurrency1 = (decimal)_paymentOverview.Where(x => x.PaymentStatus == PaymentStatus.Completed).Sum(x => x.AmmountUSD)!;
            _countcurrency2 = (decimal)_paymentOverview.Where(x => x.PaymentStatus == PaymentStatus.Completed).Sum(x => x.Ammount)!;
            _countcurrency3 = (decimal)_paymentOverview.Where(x => x.PaymentStatus == PaymentStatus.RequestPreparation).Sum(x => x.AmmountUSD)!;
            _countcurrency4 = (decimal)_paymentOverview.Where(x => x.PaymentStatus == PaymentStatus.RequestPreparation).Sum(x => x.Ammount)!;

            _countcurrency5 = (decimal)_paymentOverview.Where(x => x.CurrencyType == CurrencyType.Usd).Where(x => x.PaymentStatus == PaymentStatus.NeedRevision).Sum(x => x.AmmountUSD)!;
            _countcurrency6 = (decimal)_paymentOverview.Where(x => x.CurrencyType == CurrencyType.Idr).Where(x => x.PaymentStatus == PaymentStatus.NeedRevision).Sum(x => x.Ammount)!;
            _countcurrency7 = (decimal)_paymentOverview.Where(x => x.CurrencyType == CurrencyType.Usd).Where(x => x.PaymentStatus == PaymentStatus.RequestToBmc).Sum(x => x.AmmountUSD)!;
            _countcurrency8 = (decimal)_paymentOverview.Where(x => x.CurrencyType == CurrencyType.Idr).Where(x => x.PaymentStatus == PaymentStatus.RequestToBmc).Sum(x => x.Ammount)!;

            _countcurrency9 = (decimal)_paymentOverview.Where(x => x.CurrencyType == CurrencyType.Usd).Where(x => x.PaymentStatus == PaymentStatus.Completed).Sum(x => x.AmmountUSD)!;
            _countcurrency10 = (decimal)_paymentOverview.Where(x => x.CurrencyType == CurrencyType.Idr).Where(x => x.PaymentStatus == PaymentStatus.Completed).Sum(x => x.Ammount)!;

            _count1 = _paymentOverview.Where(x => x.PaymentStatus == PaymentStatus.Completed).Sum(x => x.CountUSD);
            _count2 = _paymentOverview.Where(x => x.PaymentStatus == PaymentStatus.Completed).Sum(x => x.CountIDR);
            _count3 = _paymentOverview.Where(x => x.PaymentStatus == PaymentStatus.RequestPreparation).Sum(x => x.CountUSD);
            _count4 = _paymentOverview.Where(x => x.PaymentStatus == PaymentStatus.RequestPreparation).Sum(x => x.CountIDR);

            _count5 = _paymentOverview.Where(x => x.CurrencyType == CurrencyType.Usd).Where(x => x.PaymentStatus == PaymentStatus.NeedRevision).Count()!;
            _count6 = _paymentOverview.Where(x => x.CurrencyType == CurrencyType.Idr).Where(x => x.PaymentStatus == PaymentStatus.NeedRevision).Count()!;
            _count7 = _paymentOverview.Where(x => x.CurrencyType == CurrencyType.Usd).Where(x => x.PaymentStatus == PaymentStatus.RequestToBmc).Count()!;
            _count8 = _paymentOverview.Where(x => x.CurrencyType == CurrencyType.Idr).Where(x => x.PaymentStatus == PaymentStatus.RequestToBmc).Count()!;

            _count9 = _paymentOverview.Where(x => x.CurrencyType == CurrencyType.Usd).Where(x => x.PaymentStatus == PaymentStatus.Completed).Count()!;
            _count10 = _paymentOverview.Where(x => x.CurrencyType == CurrencyType.Idr).Where(x => x.PaymentStatus == PaymentStatus.Completed).Count()!;

        }
        else
        {
            _error = response.Error;
            _isLoadingData = false;
        }

        _isLoadingData = false;
    }
    //public async Task OnCommandClicked(CommandClickEventArgs<GetPaymentOverviewResponse> args)
    //{
    //    if (args.CommandColumn.Type == CommandButtonType.None)
    //    {
    //        if (args.RowData.PaymentDate is not null)
    //        {
    //            var dateOnly = (DateOnly)args.RowData.PaymentDate;
    //            var request = new GetPaymentOverviewDetailRequest()
    //            {
    //                ApplicantId = args.RowData.ApplicantId,
    //                CuranyType = args.RowData.CurrencyType,
    //                WageType = args.RowData.WageType,
    //                RequestId = args.RowData.RequestId,
    //                PaymentDate = dateOnly.ToDateTime(TimeOnly.Parse("00:00 PM"))
    //            };
    //            var responseDetail = await _paymentService.GetPaymentOverviewDetailAsync(request);
    //            if (responseDetail.Error is null)
    //            {
    //                _paymentOverviewDetail = responseDetail.Result!.Items
    //                    .Where(x => x.Currency == args.RowData.CurrencyType.GetDescription())
    //                    .Where(x => x.PaymentDate == args.RowData.PaymentDate)
    //                    .Where(x => x.RequestID == args.RowData.RequestId)
    //                    .ToList();
    //            }
    //        }
    //        else
    //        {
    //            var request = new GetPaymentOverviewDetailRequest()
    //            {
    //                ApplicantId = args.RowData.ApplicantId,
    //                CuranyType = args.RowData.CurrencyType,
    //                WageType = args.RowData.WageType,
    //                RequestId = args.RowData.RequestId,
    //                PaymentDate = null
    //            };
    //            var responseDetail = await _paymentService.GetPaymentOverviewDetailAsync(request);
    //            if (responseDetail.Error is null)
    //            {
    //                _paymentOverviewDetail = responseDetail.Result!.Items
    //                    .Where(x => x.Currency == args.RowData.CurrencyType.GetDescription())
    //                    .Where(x => x.PaymentDate == args.RowData.PaymentDate)
    //                    .Where(x => x.RequestID == args.RowData.RequestId)
    //                    .ToList();
    //            }
    //        }

    //        var defaultDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15);

    //        _requestID = args.RowData.RequestId;
    //        _wageType = args.RowData.WageType;
    //        _curency = args.RowData.CurrencyType.GetDescription();
    //        _ammount = args.RowData.Ammount;
    //        _assignmentNumber = args.RowData.AssigmentNumber;
    //        _costCenter = args.RowData.CostCenter;
    //        _employeeId = args.RowData.EmployeeNumber;
    //        _username = args.RowData.Name;
    //        _paymentStatus = args.RowData.PaymentStatus;
    //        _paymentOverviewRequest.ApplicantId = args.RowData.ApplicantId;
    //        _paymentOverviewRequest.CuranyType = (int)args.RowData.CurrencyType;
    //        _paymentOverviewRequest.Id = args.RowData.Id;
    //        _paymentOverviewRequest.PaymentDate = args.RowData.PaymentDate == null ? DateOnly.FromDateTime(defaultDate) : args.RowData.PaymentDate;
    //        _paymentOverviewRequest.Notes = args.RowData.Notes;
    //        _paymentOverviewRequest.WageType = args.RowData.WageType;
    //        VisiblePopup = true;
    //        IsEnabled = true;
    //    }
    //}

    public async Task OnCommandClicked(CommandClickEventArgs<GetPaymentOverviewResponse> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.None)
        {
            var requestId = args.RowData.RequestId;
            _navigationManager.NavigateTo(RouteFor.PaymentOverviewDetail(requestId));
        }
    }
    private void DialogOpen()
    {
        VisiblePopup = true;
    }

    private void DialogClose()
    {
        VisiblePopup = false;
    }

    private async Task ClickedNo()
    {
        VisiblePopup = false;
    }
    private async Task ClickedYes()
    {
        _paymentOverviewRequest.PaymentStatus = (int)_paymentStatus;
        var response = await _paymentService.UpdateOverviewAsync(_paymentOverviewRequest);
        if (response.Error is not null)
        {
            _snackbar.AddError(response.Error.Detail);
            VisiblePopup = false;
        }
        else
        {
            _snackbar.AddSuccess($"Success Update Payment Date");
            VisiblePopup = false;
            await SetDataOnLoadAsync();
            StateHasChanged();
        }
    }

    private async Task OnClickContinueApprover()
    {
        var request = new PaymentOverviewStatusRequest()
        {
            PaymentDate = _paymentOverviewRequest.PaymentDate,
            Currancy = _paymentOverviewRequest.CuranyType,
            ApplicantId = _paymentOverviewRequest.ApplicantId,
            OldPaymentStatus = (int)_paymentStatus,
            WageType = _paymentOverviewRequest.WageType,
            PaymentStatus = 1
        };
        var response = await _paymentService.UpdateOverviewStatusAsync(request);
        if (response.Error is not null)
        {
            _snackbar.AddError(response.Error.Detail);
            VisiblePopup = false;
        }
        else
        {
            _snackbar.AddSuccess($"Success Update Payment Status");
            VisiblePopup = false;
            await SetDataOnLoadAsync();
            StateHasChanged();
        }
    }
    private async Task OnClickAskToRevise()
    {
        var request = new PaymentOverviewStatusRequest()
        {
            PaymentDate = _paymentOverviewRequest.PaymentDate,
            Currancy = _paymentOverviewRequest.CuranyType,
            ApplicantId = _paymentOverviewRequest.ApplicantId,
            OldPaymentStatus = (int)_paymentStatus,
            WageType = _paymentOverviewRequest.WageType,
            PaymentStatus = 14
        };
        var response = await _paymentService.UpdateOverviewStatusAsync(request);
        if (response.Error is not null)
        {
            _snackbar.AddError(response.Error.Detail);
            VisiblePopup = false;
        }
        else
        {
            _snackbar.AddSuccess($"Success Update Payment Status");
            VisiblePopup = false;
            await SetDataOnLoadAsync();
            StateHasChanged();
        }
    }
    private async Task OnClickApprove()
    {
        var request = new PaymentOverviewStatusRequest()
        {
            PaymentDate = _paymentOverviewRequest.PaymentDate,
            Currancy = _paymentOverviewRequest.CuranyType,
            ApplicantId = _paymentOverviewRequest.ApplicantId,
            OldPaymentStatus = (int)_paymentStatus,
            WageType = _paymentOverviewRequest?.WageType,
            PaymentStatus = 2
        };
        var response = await _paymentService.UpdateOverviewStatusAsync(request);
        if (response.Error is not null)
        {
            _snackbar.AddError(response.Error.Detail);
            VisiblePopup = false;
        }
        else
        {
            _snackbar.AddSuccess($"Success Update Payment Status");
            VisiblePopup = false;
            await SetDataOnLoadAsync();
            StateHasChanged();
        }
    }

    //private async Task OnClickDownloadAsync(MouseEventArgs e)
    //{
    //    using var excelEngine = new ExcelEngine();
    //    var application = excelEngine.Excel;
    //    application.DefaultVersion = ExcelVersion.Xlsx;

    //    //Create a workbook
    //    var workbook = application.Workbooks.Create(1);
    //    var worksheet = workbook.Worksheets[0];
    //    var headingStyle = workbook.Styles.Add("HeadingStyle");
    //    headingStyle.Font.Bold = true;
    //    worksheet.Range["A1"].ColumnWidth = 30;
    //    worksheet.Range["B1"].ColumnWidth = 30;
    //    worksheet.Range["C1"].ColumnWidth = 20;
    //    worksheet.Range["D1"].ColumnWidth = 40;
    //    worksheet.Range["D1"].WrapText = true;
    //    worksheet.Range["E1"].ColumnWidth = 20;
    //    worksheet.Range["F1"].ColumnWidth = 20;
    //    worksheet.Range["G1"].ColumnWidth = 20;
    //    worksheet.Range["H1"].ColumnWidth = 20;
    //    worksheet.Range["H1"].WrapText = true;
    //    worksheet.Range["I1"].ColumnWidth = 20;
    //    worksheet.Range["J1"].ColumnWidth = 30;
    //    worksheet.Range["J1"].WrapText = true;
    //    worksheet.Range["K1"].ColumnWidth = 30;
    //    worksheet.Range["L1"].ColumnWidth = 10;

    //    var table = DataTableExportDocumnet();

    //    //import data from datatable
    //    worksheet.ImportDataTable(table, true, 1, 1, true);

    //    //save document as a stream and retrun the stream.
    //    var downloadStream = new MemoryStream();
    //    workbook.SaveAs(downloadStream, ExcelSaveType.SaveAsXLS);
    //    workbook.Close();

    //    var content = downloadStream.ToArray();
    //    //download document
    //    await _jsRuntime.InvokeVoidAsync
    //        (JavaScriptIdentifierFor.DownloadFile,
    //        $"TamplatePaymentFinalRequest_{DateTime.Now:ddMMyyyy}.xlsx",
    //        $"{ContentTypes.ApplicationOpenXmlExcel}",
    //        content);
    //}
    //private DataTable DataTableExportDocumnet()
    //{
    //    var table = new DataTable();

    //    table.Columns.Add("Nopek", typeof(string));
    //    table.Columns.Add("Nama Pekerja", typeof(string));
    //    table.Columns.Add("Date Of Origin", typeof(string));
    //    table.Columns.Add("Net", typeof(string));
    //    table.Columns.Add("Gross", typeof(string));
    //    table.Columns.Add("Currency", typeof(string));
    //    table.Columns.Add("Amount", typeof(string));
    //    table.Columns.Add("Assignment Number", typeof(string));
    //    table.Columns.Add("Cost Center", typeof(string));
    //    table.Columns.Add("Keterangan", typeof(string));
    //    table.Columns.Add("Request ID", typeof(string));
    //    table.Columns.Add("IsComplited", typeof(string));

    //    if (_paymentOverview is not null)
    //    {
    //        var dataPaymentOverview = _paymentOverview.Where(x => x.PaymentStatus == Base.Payments.Enums.PaymentStatus.RequestToBmc).ToList();
    //        foreach (var item in dataPaymentOverview)
    //        {
    //            table.Rows.Add(
    //                item.EmployeeNumber,
    //                item.Name,
    //                item.PaymentDate!.Value.ToString("dd/MM/yyyy"),
    //                item.WageType,
    //                "",
    //                item.CurrencyType.GetDescription(),
    //                item.Ammount,
    //                item.AssigmentNumber,
    //                item.CostCenter,
    //                item.Notes,
    //                "",
    //                false
    //                );
    //        }
    //    }

    //    return table;
    //}

    //private async Task OnClickDownloadForBMCAsync(MouseEventArgs e)
    //{
    //    using var excelEngine = new ExcelEngine();
    //    var application = excelEngine.Excel;
    //    application.DefaultVersion = ExcelVersion.Xlsx;

    //    //Create a workbook
    //    var workbook = application.Workbooks.Create(1);
    //    var worksheet = workbook.Worksheets[0];
    //    var headingStyle = workbook.Styles.Add("HeadingStyle");
    //    headingStyle.Font.Bold = true;
    //    worksheet.Range["A1"].ColumnWidth = 30;
    //    worksheet.Range["B1"].ColumnWidth = 30;
    //    worksheet.Range["C1"].ColumnWidth = 20;
    //    worksheet.Range["D1"].ColumnWidth = 40;
    //    worksheet.Range["D1"].WrapText = true;
    //    worksheet.Range["E1"].ColumnWidth = 20;
    //    worksheet.Range["F1"].ColumnWidth = 20;
    //    worksheet.Range["G1"].ColumnWidth = 20;
    //    worksheet.Range["H1"].ColumnWidth = 20;
    //    worksheet.Range["H1"].WrapText = true;
    //    worksheet.Range["I1"].ColumnWidth = 20;
    //    worksheet.Range["J1"].ColumnWidth = 30;
    //    worksheet.Range["J1"].WrapText = true;

    //    worksheet.Range["A1"].HorizontalAlignment = ExcelHAlign.HAlignCenter;
    //    worksheet.Range["A1"].Text = "IT 0015 Additional Payment";
    //    worksheet.Range["A1"].CellStyle.Font.Bold = true;
    //    worksheet.Range["A1"].CellStyle.Font.Size = 14;
    //    worksheet.Range["A1:J1"].Merge();

    //    worksheet.Range["A2"].HorizontalAlignment = ExcelHAlign.HAlignCenter;
    //    worksheet.Range["A2"].Text = "Personal Data";
    //    worksheet.Range["A2"].CellStyle.Font.Bold = true;
    //    worksheet.Range["A2:B2"].Merge();

    //    worksheet.Range["C2"].HorizontalAlignment = ExcelHAlign.HAlignCenter;
    //    worksheet.Range["C2"].Text = "Create Additional Payment";
    //    worksheet.Range["C2"].CellStyle.Font.Bold = true;
    //    worksheet.Range["C2:J2"].Merge();

    //    worksheet.Range["D3"].HorizontalAlignment = ExcelHAlign.HAlignCenter;
    //    worksheet.Range["D3"].Text = "Wage Type";
    //    worksheet.Range["D3"].CellStyle.Font.Bold = true;
    //    worksheet.Range["D3:E3"].Merge();

    //    worksheet.Range["A3:A4"].Merge();
    //    worksheet.Range["B3:B4"].Merge();
    //    worksheet.Range["C3:C4"].Merge();
    //    worksheet.Range["F3:F4"].Merge();
    //    worksheet.Range["G3:G4"].Merge();
    //    worksheet.Range["H3:H4"].Merge();
    //    worksheet.Range["I3:I4"].Merge();
    //    worksheet.Range["J3:J4"].Merge();

    //    worksheet.Range["A3"].Text = "Nopek";
    //    worksheet.Range["A3"].CellStyle.Font.Bold = true;
    //    worksheet.Range["A3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
    //    worksheet.Range["A3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
    //    worksheet.Range["B3"].Text = "Nama Pekerja";
    //    worksheet.Range["B3"].CellStyle.Font.Bold = true;
    //    worksheet.Range["B3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
    //    worksheet.Range["B3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
    //    worksheet.Range["C3"].Text = "Date Of Origin";
    //    worksheet.Range["C3"].CellStyle.Font.Bold = true;
    //    worksheet.Range["C3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
    //    worksheet.Range["C3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
    //    worksheet.Range["D4"].Text = "Net";
    //    worksheet.Range["D4"].CellStyle.Font.Bold = true;
    //    worksheet.Range["E4"].Text = "Gross";
    //    worksheet.Range["E4"].CellStyle.Font.Bold = true;
    //    worksheet.Range["F3"].Text = "Currency";
    //    worksheet.Range["F3"].CellStyle.Font.Bold = true;
    //    worksheet.Range["F3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
    //    worksheet.Range["F3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
    //    worksheet.Range["G3"].Text = "Amount";
    //    worksheet.Range["G3"].CellStyle.Font.Bold = true;
    //    worksheet.Range["G3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
    //    worksheet.Range["G3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
    //    worksheet.Range["H3"].Text = "Assignment Number";
    //    worksheet.Range["H3"].CellStyle.Font.Bold = true;
    //    worksheet.Range["H3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
    //    worksheet.Range["H3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
    //    worksheet.Range["I3"].Text = "Cost Center";
    //    worksheet.Range["I3"].CellStyle.Font.Bold = true;
    //    worksheet.Range["I3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
    //    worksheet.Range["I3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
    //    worksheet.Range["J3"].Text = "Keterangan";
    //    worksheet.Range["J3"].CellStyle.Font.Bold = true;
    //    worksheet.Range["J3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
    //    worksheet.Range["J3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;

    //    var table = DataTableExport();

    //    //import data from datatable
    //    worksheet.ImportDataTable(table, true, 4, 1, true);

    //    //save document as a stream and retrun the stream.
    //    var downloadStream = new MemoryStream();
    //    workbook.SaveAs(downloadStream, ExcelSaveType.SaveAsXLS);
    //    workbook.Close();

    //    var content = downloadStream.ToArray();
    //    var request = new PaymentOverviewStatusRequest()
    //    {
    //        Stage = 2,
    //        OldPaymentStatus = 2,
    //        PaymentStatus = 12
    //    };
    //    var response = await _paymentService.UpdateOverviewStatusAsync(request);
    //    if (response.Error is not null)
    //    {
    //        _snackbar.AddError(response.Error.Detail);
    //        VisiblePopup = false;
    //    }
    //    else
    //    {
    //        _snackbar.AddSuccess($"Success Update Payment Status");
    //        VisiblePopup = false;
    //        await SetDataOnLoadAsync();
    //        StateHasChanged();
    //    }
    //    //download document
    //    await _jsRuntime.InvokeVoidAsync
    //        (JavaScriptIdentifierFor.DownloadFile,
    //        $"PaymentFormRequesttoBmc_{DateTime.Now:yyyyMMdd}.xlsx",
    //        $"{ContentTypes.ApplicationOpenXmlExcel}",
    //        content);
    //}
    //private DataTable DataTableExport()
    //{
    //    var table = new DataTable();

    //    table.Columns.Add("Nopek", typeof(string));
    //    table.Columns.Add("Nama Pekerja", typeof(string));
    //    table.Columns.Add("Date Of Origin", typeof(string));
    //    table.Columns.Add("Net", typeof(string));
    //    table.Columns.Add("Gross", typeof(string));
    //    table.Columns.Add("Currency", typeof(string));
    //    table.Columns.Add("Amount", typeof(string));
    //    table.Columns.Add("Assignment Number", typeof(string));
    //    table.Columns.Add("Cost Center", typeof(string));
    //    table.Columns.Add("Keterangan", typeof(string));

    //    if (_paymentOverview is not null)
    //    {
    //        var dataPaymentOverview = _paymentOverview.Where(x => x.PaymentStatus == Base.Payments.Enums.PaymentStatus.Approved).ToList();
    //        foreach (var item in dataPaymentOverview)
    //        {
    //            table.Rows.Add(
    //                item.EmployeeNumber,
    //                item.Name,
    //                item.PaymentDate!.Value.ToString("dd/MM/yyyy"),
    //                item.WageType,
    //                "",
    //                item.CurrencyType.GetDescription(),
    //                item.Ammount,
    //                item.AssigmentNumber,
    //                item.CostCenter,
    //                item.Notes);
    //        }
    //    }

    //    return table;
    //}

    //private async Task ValueChangeUploaderApplicants(UploadChangeEventArgs args)
    //{
    //    foreach (var file in args.Files)
    //    {
    //        var excelEngine = new ExcelEngine();
    //        var application = excelEngine.Excel;
    //        application.DefaultVersion = ExcelVersion.Excel2016;

    //        var memoryStream = new MemoryStream();
    //        await args.Files[0].File.OpenReadStream(long.MaxValue).CopyToAsync(memoryStream);
    //        memoryStream.Position = 0; // Reset the stream position

    //        var workbook = application.Workbooks.Open(memoryStream);

    //        //get the first worksheet in the workbook into IWorksheet
    //        var worksheet = workbook.Worksheets[0];

    //        _table = worksheet.ExportDataTable(worksheet.UsedRange, ExcelExportDataTableOptions.ColumnNames);

    //        //convert to list
    //        try
    //        {
    //            GenerateListFromTable(_table);
    //        }
    //        catch (Exception ex)
    //        {
    //            _snackbar.AddError(ex.Message);
    //        }
    //    }
    //}
    //protected DataTable _table = new();
    //protected List<PaymentOverviewStatusRequest> _workbookApplicants = new();

    //protected void GenerateListFromTable(DataTable table)
    //{
    //    if (table is not null)
    //    {
    //        foreach (DataRow row in table.Rows)
    //        {
    //            _workbookApplicants.Add(
    //                new PaymentOverviewStatusRequest
    //                {
    //                    EmployeeNumber = (string?)row["Nopek"],
    //                    PaymentDate = DateOnly.FromDateTime(
    //                    DateTime.ParseExact((string?)row["Date Of Origin"],
    //                                     "dd/MM/yyyy",
    //                                     CultureInfo.InvariantCulture)),

    //                    Currancy = GetStatusForUpload((string?)row["Currency"]),
    //                    RequestId = (string?)row["Request ID"],
    //                    PaymentStatus = (string?)row["IsComplited"] == "TRUE" ? 13 : 12,
    //                });
    //        }
    //    }
    //}

    //private int GetStatusForUpload(string status)
    //{
    //    var result = 0;
    //    if (status == "IDR")
    //    {
    //        result = 1;
    //    }
    //    else if (status == "USD")
    //    {
    //        result = 2;
    //    }

    //    return result;
    //}

    //protected void OnRemoveUploaderApplicants(RemovingEventArgs args)
    //{
    //}
    //private async Task OnClickUploadAsync(MouseEventArgs e)
    //{
    //    foreach (var item in _workbookApplicants)
    //    {
    //        item.Stage = 1;
    //        var response = await _paymentService.UpdateOverviewStatusAsync(item);
    //        if (response.Error is not null)
    //        {
    //            _snackbar.AddError(response.Error.Detail);
    //            VisiblePopup = false;
    //            return;
    //        }
    //    }

    //    _snackbar.AddSuccess($"Success Update Payment Status");
    //    VisiblePopup = false;
    //    await SetDataOnLoadAsync();
    //    StateHasChanged();
    //}

    private async Task ExportExcel()
    {
        var exportProperties = new ExcelExportProperties
        {
            IncludeTemplateColumn = true
        };
        await Grid.ExportToExcelAsync(exportProperties);
    }
    public async Task ToolbarClickHandler(ClickEventArgs args)
    {
        if (args.Item.Id.Contains("_excelexport")) //Id is combination of Grid's ID and itemname
        {
            var exportProperties = new ExcelExportProperties
            {
                IncludeTemplateColumn = true
            };
            await Grid.ExportToExcelAsync(exportProperties);
        }
    }
    public void ExcelQueryCellInfoHandler(ExcelQueryCellInfoEventArgs<GetPaymentOverviewResponse> args)
    {
        var a = args.Column.Field;
        if (args.Column.Field == "CurrencyType")
        {
            args.Cell.Value = args.Data.CurrencyType.GetDescription();
        }
        else if (args.Column.Field == "Ammount")
        {
            args.Cell.Value = args.Data.Ammount?.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
        }
        else if (args.Column.Field == "PaymentStatus")
        {
            args.Cell.Value = args.Data.PaymentStatus.GetDescription().ToString();
        }
    }
    //private async Task OnClickApproveAsync(MouseEventArgs args)
    //{
    //    var request = new PaymentOverviewStatusRequest()
    //    {
    //        OldPaymentStatus = (int)_paymentStatus,
    //        PaymentStatus = 11
    //    };
    //    var response = await _paymentService.UpdateOverviewStatusAsync(request);
    //    if (response.Error is not null)
    //    {
    //        _snackbar.AddError(response.Error.Detail);
    //        VisiblePopup = false;
    //    }
    //    else
    //    {
    //        _snackbar.AddSuccess($"Success Update Payment Status");
    //        VisiblePopup = false;
    //        await SetDataOnLoadAsync();
    //        StateHasChanged();
    //    }
    //}
}
