using Microsoft.AspNetCore.Components;
using Smlp.Shared.Scholarships.ScholarshipPosting.Queries;
using Syncfusion.Blazor.DropDowns;

namespace Smlp.Bsui.Common.Components;

public partial class DropdownScholarship
{
    [Parameter]
    public EventCallback<ChangeEventArgs<Guid, GetScholarshipPostingResponse>> OnChangeAsync { get; set; }

    protected List<GetScholarshipPostingResponse> _scholarships = new();

    protected override async Task OnInitializedAsync()
    {
        var request = new GetScholarshipPostingRequest()
        {
            CompanyCode = "0000",
        };
        var scholarships = await _scholarshipPostingService.GetScholarshipPostingAsync(request);
        if (scholarships.Error is null)
        {
            _scholarships = scholarships.Result!.Items.ToList();
            //_scholarships.Add(new GetScholarshipPostingResponse { Id = Guid.Empty, TitleBatch = "All Batch", DueDate = DateTimeOffset.MaxValue });

            _scholarships = _scholarships.OrderByDescending(x => x.DueDate).ToList();
        }
    }
}
