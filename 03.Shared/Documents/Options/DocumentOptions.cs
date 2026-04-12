namespace Smlp.Shared.Documents.Options;

public class DocumentOptions
{
    public const string SectionKey = nameof(Documents);

    public long MaximumFileSizeInBytes { get; set; }
    public string FolderPath { get; set; } = default!;
    public DocumentDraftId DocumentDraftId { get; set; } = default!;
}

public class DocumentDraftId
{
    public ScholarshipPlan ScholarshipPlan { get; set; } = default!;
    public SelectionOpen SelectionOpen { get; set; } = default!;
    public SelectionClose SelectionClose { get; set; } = default!;
    public SturyPreparation SturyPreparation { get; set; } = default!;
    public UniversityRegistration UniversityRegistration { get; set; } = default!;
    public Payment Payment { get; set; } = default!;
}

public class ScholarshipPlan
{
    public string Approval { get; set; } = default!;
}

public class SelectionOpen
{
    public string BeritaAcara { get; set; } = default!;
    public string SK { get; set; } = default!;
    public string SP { get; set; } = default!;
}

public class SelectionClose
{
    public string BeritaAcara { get; set; } = default!;
    public string SK { get; set; } = default!;
    public string SP { get; set; } = default!;
}

public class SturyPreparation
{
    public string SponshorshipLetter { get; set; } = default!;
    public string VisaGuarantee { get; set; } = default!;
}

public class UniversityRegistration
{
    public string Aggrement { get; set; } = default!;
    public string Warrant { get; set; } = default!;
}

public class Payment
{
    public string TuitionFee { get; set; } = default!;
}

