using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Smlp.Base.Universities.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Extensions;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Masters.Queries;
using Smlp.Shared.Universities.UniversityRecommendation.Commands.Create;
using Smlp.Shared.Universities.UniversityRecommendation.Commands.Update;
using Smlp.Shared.Universities.UniversityRecommendation.Queries;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Navigations;
using Syncfusion.XlsIO;

namespace Smlp.Bsui.Features.Universities.Recommendation;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;

    #region Protected Properties
    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;

    protected List<GetScholarshipSubjectQueryResponse> _subjects = new();
    protected List<GetUniversityRecommendationQueryResponse> _universityRecommendations = new();
    private SfGrid<GetUniversityRecommendationQueryResponse> Grid { get; set; } = new();

    protected CreateUniversityRecommendationRequest _createUniversityRecommendationRequest = new();
    protected UpdateUniversityRecommendationRequest _updateUniversityRecommendationRequest = new();

    protected UniversityCategory _category = 0;
    protected int _subject = 0;
    protected int _year = 0;

    protected DataTable _tableExcel = new();
    protected List<GetUniversityRecommendationQueryResponse> _workbookExcel = new();
    protected long _maximumFileSizeInBytes = long.MaxValue;
    protected SfUploader? _uploadObj;
    #endregion

    #region Private Properties
    private bool _isLoadingDialog = false;
    private string? ActionType { get; set; }
    private bool IsEnabled { get; set; }
    private bool VisiblePopup { get; set; }
    private string TitlePopup { get; set; } = "Add";

    private bool VisiblePopupDelete { get; set; } = false;
    protected Guid IdDeleted { get; set; } = Guid.Empty;
    #endregion

    #region Dropdown Properties
    public List<DropdownEnumResponse<UniversityCategory>> _dropdownEnumCategories = new();
    public List<DropdownItemResponse> _dropdownYear = new();
    #endregion

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        _isLoading = true;
        await SetDataOnLoadAsync();
        await GetDropdownCategories();
        await GetDropdownSubject();
        await GetDropdownYear();
        _isLoading = false;

    }

    #region Data Grid
    private async Task SetDataOnLoadAsync()
    {
        _isLoadingData = true;
        var data = await _universityRecommendationService.GetUniversityRecommendationAsync();
        if (data.Error is null)
        {
            _universityRecommendations = data.Result!.Items.ToList();
            if (_category != 0)
            {
                _universityRecommendations = _universityRecommendations.Where(x => x.UniversityCategory == _category).ToList();
            }

            if (_subject != 0)
            {
                _universityRecommendations = _universityRecommendations.Where(x => x.ScholarshipSubject.Id == _subject).ToList();
            }

            if (_year != 0)
            {
                _universityRecommendations = _universityRecommendations.Where(x => x.Dateline.Year == _year).ToList();
            }
        }
        else
        {
            _error = data.Error;
        }

        _isLoading = false;
    }

    public void RowBound(RowDataBoundEventArgs<GetUniversityRecommendationQueryResponse> args)
    {
        if (args.Data.Admission is not null && args.Data.Admission!.Count() > 0)
        {
            args.Row.AddClass(new string[] { "e-removeDeletecommand" });
        }
    }
    #endregion

    #region Dropdown
    private async Task GetDropdownCategories()
    {
        _dropdownEnumCategories = EnumExtentions.ToDropdownEnum<UniversityCategory>();
        await Task.CompletedTask;
    }

    private async Task GetDropdownSubject()
    {
        var subjects = await _scholarshipSubjectService.GetScholarshipSubjectsAsync();
        if (subjects.Error is null)
        {
            _subjects = subjects.Result!.Items.ToList();
            _subjects = _subjects.OrderBy(x => x.Name).ToList();
        }

        await Task.CompletedTask;
    }

    private async Task GetDropdownYear()
    {
        _dropdownYear = _universityRecommendations.Select(x => new DropdownItemResponse { Id = x.Dateline.Year, Description = x.Dateline.Year.ToString() }).DistinctBy(x => x.Id).ToList();
        await Task.CompletedTask;
    }
    #endregion

    #region OnClick Event
    private void OnClickAddAsync()
    {
        TitlePopup = "Add";
        ActionType = "add";
        IsEnabled = true;
        VisiblePopup = true;
        _createUniversityRecommendationRequest.Dateline = DateOnly.FromDateTime(DateTime.Now);
        _createUniversityRecommendationRequest.ScholarshipSubject = new Domain.Entities.Masters.ScholarshipSubject();
    }
    #endregion

    #region Excel
    private async Task OnClickUploadExcelAsync(MouseEventArgs e)
    {
        if (_workbookExcel.Count() == 0)
        {
            _snackbar.AddWarning("Please browse file");
            return;
        }

        _isLoadingDialog = true;
        if (_workbookExcel.Count() > 0)
        {
            var table = new DataTable();

            table.Columns.Add("Id", typeof(string));
            table.Columns.Add("University", typeof(string));
            table.Columns.Add("Country", typeof(string));
            table.Columns.Add("OfficialWebsite", typeof(string));
            table.Columns.Add("RegistrationLink", typeof(string));
            table.Columns.Add("UniversityCategory", typeof(string));
            table.Columns.Add("ScholarshipSubject", typeof(string));
            table.Columns.Add("Deadline", typeof(DateOnly));
            table.Columns.Add("Message", typeof(string));

            foreach (var item in _workbookExcel)
            {
                var message = new List<string>();

                if (string.IsNullOrEmpty(item.University))
                {
                    message.Add("University is required");
                }

                if (string.IsNullOrEmpty(item.Country))
                {
                    message.Add("Country is required");
                }

                if (string.IsNullOrEmpty(item.OfficialWebsite))
                {
                    message.Add("Official Website is required");
                }

                if (string.IsNullOrEmpty(item.RegistrationLink))
                {
                    message.Add("Registration Link is required");
                }

                if (item.UniversityCategory == 0)
                {
                    message.Add("University must : " + string.Join(" or ", Enum.GetNames(typeof(UniversityCategory))));
                }

                if (item.ScholarshipSubject.Id == 0)
                {
                    message.Add("Scholarship Subject is not found");
                }

                if (message.Count() > 0)
                {
                    table.Rows.Add(
                        item.Id,
                        item.University,
                        item.Country,
                        item.OfficialWebsite,
                        item.RegistrationLink,
                        item.UniversityCategory,
                        item.ScholarshipSubject.Name,
                        item.Dateline,
                        string.Join(", ", message)
                    );

                    continue;
                }

                if (item.Id == Guid.Empty)
                {
                    #region Insert
                    var createData = new CreateUniversityRecommendationRequest
                    {
                        University = item.University,
                        Country = item.Country,
                        OfficialWebsite = item.OfficialWebsite,
                        RegistrationLink = item.RegistrationLink,
                        ScholarshipSubject = item.ScholarshipSubject,
                        UniversityCategory = item.UniversityCategory,
                        Dateline = item.Dateline,
                    };
                    var createDataResponse = await _universityRecommendationService.CreateUniversityRecommendationAsync(createData);
                    if (createDataResponse.Error is not null)
                    {
                        table.Rows.Add(
                            item.Id,
                            item.University,
                            item.Country,
                            item.OfficialWebsite,
                            item.RegistrationLink,
                            item.UniversityCategory,
                            item.ScholarshipSubject.Name,
                            item.Dateline,
                            string.Join(", ", createDataResponse.Error.Details)
                        );
                    }
                    #endregion
                }
                else
                {
                    #region Update
                    var updateData = new UpdateUniversityRecommendationRequest
                    {
                        Id = item.Id,
                        University = item.University,
                        Country = item.Country,
                        OfficialWebsite = item.OfficialWebsite,
                        RegistrationLink = item.RegistrationLink,
                        ScholarshipSubject = item.ScholarshipSubject,
                        UniversityCategory = item.UniversityCategory,
                        Dateline = item.Dateline,
                    };
                    var updateDataResponse = await _universityRecommendationService.UpdateUniversityRecommendationAsync(updateData);
                    if (updateDataResponse.Error is not null)
                    {
                        table.Rows.Add(
                            item.Id,
                            item.University,
                            item.Country,
                            item.OfficialWebsite,
                            item.RegistrationLink,
                            item.UniversityCategory,
                            item.ScholarshipSubject.Name,
                            item.Dateline,
                            string.Join(", ", updateDataResponse.Error.Details)
                        );
                    }
                    #endregion
                }
            }

            // generate new excel
            if (table.Rows.Count > 0)
            {
                await GenerateExcel(table, $"Validation_UniversityRecommendation_{DateTime.Now:yyyyMMdd}.xlsx");
            }

            _snackbar.AddSuccess("Success upload data");
            _isLoadingDialog = false;
            await _uploadObj!.ClearAllAsync();
            _workbookExcel = new();
            await SetDataOnLoadAsync();
            StateHasChanged();
        }
    }

    private async Task OnClickDownloadExcelAsync(MouseEventArgs e)
    {
        _isLoadingDialog = true;

        var table = new DataTable();

        table.Columns.Add("Id", typeof(string));
        table.Columns.Add("University", typeof(string));
        table.Columns.Add("Country", typeof(string));
        table.Columns.Add("OfficialWebsite", typeof(string));
        table.Columns.Add("RegistrationLink", typeof(string));
        table.Columns.Add("UniversityCategory", typeof(string));
        table.Columns.Add("ScholarshipSubject", typeof(string));
        table.Columns.Add("Deadline", typeof(DateOnly));

        if (_universityRecommendations is not null)
        {
            foreach (var item in _universityRecommendations)
            {
                table.Rows.Add(
                    item.Id,
                    item.University,
                    item.Country,
                    item.OfficialWebsite,
                    item.RegistrationLink,
                    item.UniversityCategory,
                    item.ScholarshipSubject.Name,
                    item.Dateline);
            }
        }

        await GenerateExcel(table);

        _isLoadingDialog = false;
        StateHasChanged();
    }

    private async Task GenerateExcel(DataTable data, string fileName = "")
    {
        using var excelEngine = new ExcelEngine();
        var application = excelEngine.Excel;
        application.DefaultVersion = ExcelVersion.Xlsx;

        //Create a workbook
        var workbook = application.Workbooks.Create(1);
        var worksheet = workbook.Worksheets[0];
        worksheet.Range[$"A1"].ColumnWidth = 40;
        worksheet.Range[$"B1"].ColumnWidth = 40;
        worksheet.Range[$"C1"].ColumnWidth = 40;
        worksheet.Range[$"D1"].ColumnWidth = 30;
        worksheet.Range[$"E1"].ColumnWidth = 40;
        worksheet.Range[$"F1"].ColumnWidth = 40;
        worksheet.Range[$"G1"].ColumnWidth = 40;
        worksheet.Range[$"H1"].ColumnWidth = 40;

        var headingStyle = workbook.Styles.Add("HeadingStyle");
        headingStyle.Font.Bold = true;
        worksheet.Range["I1:I10000"].CellStyle = headingStyle;

        worksheet.Protect("sch0larship");
        worksheet.Range["B2:I10000"].CellStyle.Locked = false;

        //Add Comments (Notes)
        IComment commentA = worksheet.Range[$"A1"].AddComment();
        commentA.Text = "Do not modify";
        commentA.IsVisible = false;

        IComment commentB = worksheet.Range[$"B1"].AddComment();
        commentB.Text = "Please entry University Name";
        commentB.IsVisible = false;

        IComment commentC = worksheet.Range[$"C1"].AddComment();
        commentC.Text = "Please Entry Country Name";
        commentC.IsVisible = false;

        IComment commentD = worksheet.Range[$"D1"].AddComment();
        commentD.Text = "Please Entry Official Website";
        commentD.IsVisible = false;

        IComment commentE = worksheet.Range[$"E1"].AddComment();
        commentE.Text = "Please Entry Registration Link";
        commentE.IsVisible = false;

        IComment commentF = worksheet.Range[$"F1"].AddComment();
        commentF.Text = "Word or Local";
        commentF.IsVisible = false;

        IComment commentG = worksheet.Range[$"G1"].AddComment();
        commentG.Text = "Scholarship Subject Name : " + string.Join(" or ", _subjects.Select(x => x.Name).ToList());
        commentG.IsVisible = false;

        IComment commentH = worksheet.Range[$"H1"].AddComment();
        commentH.Text = "Please Entry Deadline use format mm/dd/yyyy";
        commentH.IsVisible = false;

        //import data from datatable
        worksheet.ImportDataTable(data, true, 1, 1, true);

        //save document as a stream and retrun the stream.
        var downloadStream = new MemoryStream();
        workbook.SaveAs(downloadStream, ExcelSaveType.SaveAsXLS);
        workbook.Close();

        var content = downloadStream.ToArray();

        //download document
        await _jsRuntime.InvokeVoidAsync
            (JavaScriptIdentifierFor.DownloadFile,
            string.IsNullOrEmpty(fileName) ? $"UniversityRecommendation_{DateTime.Now:yyyyMMdd}.xlsx" : fileName,
            $"{ContentTypes.ApplicationOpenXmlExcel}",
            content);

    }

    private async Task ValueChangeUploaderExcel(UploadChangeEventArgs args)
    {
        foreach (var file in args.Files)
        {
            _workbookExcel = new();
            var excelEngine = new ExcelEngine();
            var application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Excel2016;

            var memoryStream = new MemoryStream();
            await args.Files[0].File.OpenReadStream(long.MaxValue).CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var workbook = application.Workbooks.Open(memoryStream);

            //get the first worksheet in the workbook into IWorksheet
            var worksheet = workbook.Worksheets[0];

            _tableExcel = worksheet.ExportDataTable(worksheet.UsedRange, ExcelExportDataTableOptions.ColumnNames);

            //convert to list
            try
            {
                if (_tableExcel is not null)
                {
                    foreach (DataRow row in _tableExcel.Rows)
                    {
                        if (row["University"] == DBNull.Value)
                        {
                            break;
                        }
                        else
                        {
                            var idSubject = _subjects.Where(x => x.Name.Equals((string)row["ScholarshipSubject"])).Select(x => x.Id).SingleOrDefault();
                            var isCategory = Enum.IsDefined(typeof(UniversityCategory), (string)row["UniversityCategory"]);

                            var item = new GetUniversityRecommendationQueryResponse
                            {
                                Id = row["Id"] == null || row["Id"] == DBNull.Value ? Guid.Empty : new Guid((string)row["Id"]),
                                University = row["University"] == null || row["University"] == DBNull.Value ? string.Empty : (string)row["University"],
                                Country = row["Country"] == null || row["Country"] == DBNull.Value ? string.Empty : (string)row["Country"],
                                OfficialWebsite = row["OfficialWebsite"] == null || row["OfficialWebsite"] == DBNull.Value ? string.Empty : (string)row["OfficialWebsite"],
                                RegistrationLink = row["RegistrationLink"] == null || row["RegistrationLink"] == DBNull.Value ? string.Empty : (string)row["RegistrationLink"],
                                Dateline = row["Deadline"] == null || row["Deadline"] == DBNull.Value ? DateOnly.Parse(DateTime.Now.ToString()) : DateOnly.Parse((string)row["Deadline"]),
                                UniversityCategory = isCategory == true ? Enum.Parse<UniversityCategory>((string)row["UniversityCategory"]) : 0,
                                ScholarshipSubject = new Domain.Entities.Masters.ScholarshipSubject { Id = idSubject, Name = (string)row["ScholarshipSubject"] }
                            };
                            _workbookExcel.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _snackbar.AddError(ex.Message);
            }
        }
    }

    protected void OnRemoveUploaderExcel(RemovingEventArgs args)
    {
        _workbookExcel = new();
        args.PostRawFile = false;
    }
    #endregion

    #region OnChange Event
    protected async Task OnChangeCategoryAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<UniversityCategory, DropdownEnumResponse<UniversityCategory>> args)
    {
        if (args.ItemData == null)
        {
            _category = 0;
        }
        else
        {
            _category = args.ItemData.Id;
        }

        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    protected async Task OnChangeSubjectAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, GetScholarshipSubjectQueryResponse> args)
    {
        _subject = args.Value;
        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    protected async Task OnChangeYearAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, DropdownItemResponse> args)
    {
        _year = args.Value;
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    #endregion

    #region Command Event
    public async Task OnCommandClicked(CommandClickEventArgs<GetUniversityRecommendationQueryResponse> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.Delete)
        {
            IdDeleted = args.RowData.Id;
            VisiblePopupDelete = true;
        }
        else if (args.CommandColumn.Type == CommandButtonType.Edit)
        {
            ActionType = "edit";
            TitlePopup = "Edit";
            IsEnabled = true;
            VisiblePopup = true;
            _createUniversityRecommendationRequest.Id = args.RowData.Id;
            _createUniversityRecommendationRequest.University = args.RowData.University;
            _createUniversityRecommendationRequest.Country = args.RowData.Country;
            _createUniversityRecommendationRequest.OfficialWebsite = args.RowData.OfficialWebsite;
            _createUniversityRecommendationRequest.RegistrationLink = args.RowData.RegistrationLink;
            _createUniversityRecommendationRequest.ScholarshipSubject = args.RowData.ScholarshipSubject;
            _createUniversityRecommendationRequest.UniversityCategory = args.RowData.UniversityCategory;
            _createUniversityRecommendationRequest.Dateline = args.RowData.Dateline;
        }
        else
        {
            TitlePopup = "View";
            IsEnabled = false;
            VisiblePopup = true;
            _createUniversityRecommendationRequest.Id = args.RowData.Id;
            _createUniversityRecommendationRequest.University = args.RowData.University;
            _createUniversityRecommendationRequest.Country = args.RowData.Country;
            _createUniversityRecommendationRequest.OfficialWebsite = args.RowData.OfficialWebsite;
            _createUniversityRecommendationRequest.RegistrationLink = args.RowData.RegistrationLink;
            _createUniversityRecommendationRequest.ScholarshipSubject = args.RowData.ScholarshipSubject;
            _createUniversityRecommendationRequest.UniversityCategory = args.RowData.UniversityCategory;
            _createUniversityRecommendationRequest.Dateline = args.RowData.Dateline;
        }

        await Task.CompletedTask;
    }
    #endregion

    #region Modal Event
    private void DialogOpen()
    {
        VisiblePopup = true;
    }

    private void DialogClose()
    {
        ActionType = "";
        VisiblePopup = false;
        _createUniversityRecommendationRequest = new CreateUniversityRecommendationRequest { Dateline = DateOnly.FromDateTime(DateTime.Now), ScholarshipSubject = new Domain.Entities.Masters.ScholarshipSubject() };
        _updateUniversityRecommendationRequest = new UpdateUniversityRecommendationRequest();
    }
    private void ClickedNo()
    {
        DialogClose();
    }

    private async Task ClickedYes()
    {
        if (ActionType == "add")
        {
            #region Create
            var createData = new CreateUniversityRecommendationRequest
            {
                University = _createUniversityRecommendationRequest.University,
                Country = _createUniversityRecommendationRequest.Country,
                OfficialWebsite = _createUniversityRecommendationRequest.OfficialWebsite,
                RegistrationLink = _createUniversityRecommendationRequest.RegistrationLink,
                ScholarshipSubject = _createUniversityRecommendationRequest.ScholarshipSubject,
                UniversityCategory = _createUniversityRecommendationRequest.UniversityCategory,
                Dateline = _createUniversityRecommendationRequest.Dateline,
            };
            var createDataResponse = await _universityRecommendationService.CreateUniversityRecommendationAsync(createData);
            if (createDataResponse.Error is not null)
            {
                var result = string.Empty;
                for (var i = 0; i < createDataResponse.Error.Details.Count; i++)
                {
                    result = result + createDataResponse.Error.Details[i] + ",";
                }

                _snackbar.AddError("University Recommendation - Error : " + result);
                _isLoadingDialog = false;
                StateHasChanged();
                return;
            }

            _snackbar.AddSuccess("Success add data");
            #endregion
        }
        else if (ActionType == "edit")
        {
            #region update
            var updateData = new UpdateUniversityRecommendationRequest
            {
                Id = _createUniversityRecommendationRequest.Id,
                University = _createUniversityRecommendationRequest.University,
                Country = _createUniversityRecommendationRequest.Country,
                OfficialWebsite = _createUniversityRecommendationRequest.OfficialWebsite,
                RegistrationLink = _createUniversityRecommendationRequest.RegistrationLink,
                ScholarshipSubject = _createUniversityRecommendationRequest.ScholarshipSubject,
                UniversityCategory = _createUniversityRecommendationRequest.UniversityCategory,
                Dateline = _createUniversityRecommendationRequest.Dateline,
            };
            var updateDataResponse = await _universityRecommendationService.UpdateUniversityRecommendationAsync(updateData);
            if (updateDataResponse.Error is not null)
            {
                var result = string.Empty;
                for (var i = 0; i < updateDataResponse.Error.Details.Count; i++)
                {
                    result = result + updateDataResponse.Error.Details[i] + ",";
                }

                _snackbar.AddError("Univesity Recommendation - Error : " + result);
                _isLoadingDialog = false;
                StateHasChanged();
                return;
            }

            _snackbar.AddSuccess("Success update data");
            #endregion
        }

        await SetDataOnLoadAsync();
        StateHasChanged();
        DialogClose();
    }
    #endregion

    #region Modal Delete Confirm
    private void DialogDeleteOpen()
    {
        VisiblePopupDelete = true;
    }

    private void DialogDeleteClose()
    {
        VisiblePopupDelete = false;
        IdDeleted = new Guid();
    }

    private void ClickedNoDelete()
    {
        VisiblePopupDelete = false;
        IdDeleted = new Guid();
    }

    private async void ClickedYesDelete()
    {
        var response = await _universityRecommendationService.DeleteUniversityRecommendationAsync(IdDeleted);
        if (response.Error is null)
        {
            _snackbar.AddSuccess("Success delete data");
            DialogDeleteClose();
            await SetDataOnLoadAsync();
            StateHasChanged();
        }
        else
        {
            var result = string.Empty;
            for (var i = 0; i < response.Error.Details.Count; i++)
            {
                result = result + response.Error.Details[i] + ",";
            }

            _snackbar.AddError("University Recommendation - Error : " + result);
        }
    }
    #endregion

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
    public void ExcelQueryCellInfoHandler(ExcelQueryCellInfoEventArgs<GetUniversityRecommendationQueryResponse> args)
    {
        var a = args.Column.Field;
        if (args.Column.Field == "OfficialWebsite")
        {
            args.Cell.Value = args.Data.OfficialWebsite;
        }
    }
}
