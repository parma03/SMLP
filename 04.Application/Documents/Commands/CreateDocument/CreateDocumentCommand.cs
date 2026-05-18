using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;
using Smlp.Application.Services.Persistence;
using Smlp.Application.Services.Storage;
using Smlp.Domain.Entities;
using Smlp.Domain.Events;
using Smlp.Shared.Documents.Commands.CreateDocument;
using Smlp.Shared.Documents.Options;

namespace Smlp.Application.Documents.Commands.CreateDocument;

//[Authorize(Policy = Permissions.Smlp_Document_Create)]
public class CreateDocumentCommand : CreateDocumentRequest, IRequest<CreateDocumentResponse>
{
}

public class CreateDocumentCommandValidator : AbstractValidator<CreateDocumentCommand>
{
    public CreateDocumentCommandValidator(IOptions<DocumentOptions> documentOptions)
    {
        Include(new CreateDocumentRequestValidator(documentOptions));
    }
}

public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, CreateDocumentResponse>
{
    private readonly ISmlpDbContext _context;
    private readonly IStorageService _storageService;

    public CreateDocumentCommandHandler(
        ISmlpDbContext context,
        IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<CreateDocumentResponse> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        var filename = string.Empty;
        using var memoryStream = new MemoryStream();
        await request.File.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;
        var file = memoryStream.ToArray();
        var timeStamp = DateTime.Now.ToString("ddMMyyyy");
        var fillTimeStamp = DateTime.Now.ToString("ddMMyyyyhhmmss");
        var extention = Path.GetExtension(request.File.FileName);

        if (request.DocumentType is Base.Selections.Enums.DocumentType.CertificateTOEFL)
        {
            filename = $"TOEFL_{request.EmployeeNumber}_{timeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.CertificatePsychotest)
        {
            filename = $"Psychotest_{request.EmployeeNumber}_{timeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.AdmissionLetterOfAcceptance)
        {
            filename = $"SuratPenerimaan_{request.EmployeeNumber}_{timeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.AdmissionSuperiorApprovalLetter)
        {
            filename = $"SuratPersetujuan_{request.EmployeeNumber}_{timeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.AdmissionAgreementLetter)
        {
            filename = $"SuratPerjanjian_{request.EmployeeNumber}_{timeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.AdmissionWarrantLetter)
        {
            filename = $"SuratPerintah_{request.EmployeeNumber}_{timeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.AdmissionAgreementApprovedLetter)
        {
            filename = $"SuratPerjanjianApproved_{request.EmployeeNumber}_{timeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.AdmissionWarrantApprovedLetter)
        {
            filename = $"SuratPerintahApproved_{request.EmployeeNumber}_{timeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.Reimbursement)
        {
            filename = $"Reimbursement_{request.EmployeeNumber}_{fillTimeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.Disbursement)
        {
            filename = $"Disbursement_{request.EmployeeNumber}_{fillTimeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.DraftTuitionFee)
        {
            filename = $"DraftTuitionFee_{request.EmployeeNumber}_{fillTimeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.FinalTuitionFee)
        {
            filename = $"FinalTuitionFee_{request.EmployeeNumber}_{fillTimeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.SponsorshipLetter)
        {
            filename = $"Sponsorship_{request.EmployeeNumber}_{fillTimeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.DraftSponsorshipLetter)
        {
            filename = $"DraftSponsorship_{request.EmployeeNumber}_{timeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.FinalSponsorshipLetter)
        {
            filename = $"FinalSponsorship_{request.EmployeeNumber}_{fillTimeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.VisaGuarantee)
        {
            filename = $"VisaGuarantee_{request.EmployeeNumber}_{timeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.DraftVisaGuarantee)
        {
            filename = $"DraftVisaGuarantee_{request.EmployeeNumber}_{timeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.FinalVisaGuarantee)
        {
            filename = $"FinalVisaGuarantee_{request.EmployeeNumber}_{timeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.GMATCertificate)
        {
            filename = $"GMAT_{request.EmployeeNumber}_{fillTimeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.IELTSCertificate)
        {
            filename = $"IELTS_{request.EmployeeNumber}_{fillTimeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.GRECertificate)
        {
            filename = $"GRE_{request.EmployeeNumber}_{fillTimeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.Essay)
        {
            filename = $"Essay_{request.EmployeeNumber}_{fillTimeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.Passport)
        {
            filename = $"Passport_{request.EmployeeNumber}_{timeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.Visa)
        {
            filename = $"Visa_{request.EmployeeNumber}_{timeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.Transcript)
        {
            filename = $"Transcript_{request.EmployeeNumber}_{fillTimeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.LastExam)
        {
            filename = $"LastExam_{request.EmployeeNumber}_{timeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.Graduation)
        {
            filename = $"Graduation_{request.EmployeeNumber}_{timeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.Completion)
        {
            filename = $"Completion_{request.EmployeeNumber}_{timeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.FinalProject)
        {
            filename = $"FinalProject_{request.EmployeeNumber}_{timeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.ProjectPlan)
        {
            filename = $"ProjectPlan_{request.EmployeeNumber}_{fillTimeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.ProjectCharterPlan)
        {
            filename = $"ProjectCharterPlan_{request.EmployeeNumber}_{fillTimeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.ProjectCharterPlanRealitation)
        {
            filename = $"ProjectCharterPlanRealitation_{request.EmployeeNumber}_{fillTimeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.BankAccount)
        {
            filename = $"BankAccount_{request.File.Name}_{fillTimeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.AfterProgram)
        {
            filename = $"AfterProgram_{request.File.Name}_{fillTimeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.DraftScholarshipPlan)
        {
            filename = $"DraftScholarshipPlan_{request.File.Name}_{fillTimeStamp}{extention}";
        }
        else if (request.DocumentType == Base.Selections.Enums.DocumentType.FinalScholarshipPlan)
        {
            filename = $"FinalScholarshipPlan_{request.File.Name}_{fillTimeStamp}{extention}";
        }
        else
        {
            filename = request.File.FileName;
        }

        try
        {
            var document = new Document
            {
                Id = Guid.NewGuid(),
                DocumentType = request.DocumentType,
                FileName = filename,
                FileSize = request.File.Length,
                FileContentType = request.File.ContentType,
                StorageFileId = await _storageService.CreateAsync(file, request.DocumentType, filename)
            };

            document.DomainEvents.Add(new DocumentCreatedEvent(document.Id));

            await _context.Documents.AddAsync(document, cancellationToken);
            await _context.SaveChangesAsync(this, cancellationToken);

            return new CreateDocumentResponse
            {
                DocumentId = document.Id
            };
        }
        catch (Exception)
        {
            throw new Exception();
        }
    }
}
