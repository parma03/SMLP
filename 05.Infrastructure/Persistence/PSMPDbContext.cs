/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Psmp.Application.Services.CurrentUser;
using Psmp.Application.Services.DateAndTime;
using Psmp.Application.Services.DomainEvent;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities;
using Psmp.Domain.Entities.Applicants;
using Psmp.Domain.Entities.Logs;
using Psmp.Domain.Entities.Masters;
using Psmp.Domain.Entities.Payments;
using Psmp.Domain.Entities.Projects;
using Psmp.Domain.Entities.Scholarships;
using Psmp.Domain.Entities.Selections;
using Psmp.Domain.Entities.StudyMonitorings;
using Psmp.Domain.Entities.StudyPreparations;
using Psmp.Domain.Entities.Universities;
using Psmp.Domain.Events;
using Psmp.Domain.Interfaces;
using Psmp.Infrastructure.Persistence.Common.Extensions;
using Psmp.Infrastructure.Persistence.Common.Models;

namespace Psmp.Infrastructure.Persistence;

public class PsmpDbContext : DbContext, IPsmpDbContext
{
    private const string SoftDeleted = nameof(SoftDeleted);

    protected ICurrentUserService _currentUser = default!;
    protected IDateAndTimeService _dateTime = default!;
    protected IDomainEventService _domainEvent = default!;

    #region Common Entities

    public DbSet<Audit> Audits => Set<Audit>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentHtml> DocumentHtmls => Set<DocumentHtml>();

    #endregion Common Entities

    #region Master Data Entities

    public DbSet<CompanyGroup> CompanyGroups => Set<CompanyGroup>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Directorate> Directorates => Set<Directorate>();
    public DbSet<Division> Divisions => Set<Division>();
    public DbSet<ScholarshipSubject> ScholarshipSubjects => Set<ScholarshipSubject>();
    public DbSet<Faculty> Faculties => Set<Faculty>();
    public DbSet<Fungsi> Fungsis => Set<Fungsi>();
    public DbSet<Degree> Degrees => Set<Degree>();
    public DbSet<AdministrativeCriteria> AdministrativeCriterias => Set<AdministrativeCriteria>();
    public DbSet<Bank> Banks => Set<Bank>();
    public DbSet<MasterSetDataStatus> MasterSetDataStatuses => Set<MasterSetDataStatus>();

    #endregion Master Data Entities

    #region Applicants

    public DbSet<Applicant> Applicants => Set<Applicant>();
    public DbSet<ApplicantByPass> ApplicantByPasses => Set<ApplicantByPass>();
    public DbSet<ApplicantAdministrativeCriteria> ApplicantAdministrativeCriterias => Set<ApplicantAdministrativeCriteria>();
    public DbSet<ApplicantTOEFLSchedule> ApplicantTOEFLSchedules => Set<ApplicantTOEFLSchedule>();
    public DbSet<ApplicantPsychotestSchedule> ApplicantPsychotestSchedules => Set<ApplicantPsychotestSchedule>();
    public DbSet<ApplicantCancel> ApplicantCancels => Set<ApplicantCancel>();
    public DbSet<ApplicantBankAccount> ApplicantBankAccounts => Set<ApplicantBankAccount>();

    #endregion Applicants

    #region Scholarships

    public DbSet<ScholarshipPlan> ScholarshipPlans => Set<ScholarshipPlan>();
    public DbSet<ScholarshipPlanApproval> ScholarshipPlanApprovals => Set<ScholarshipPlanApproval>();
    public DbSet<ScholarshipPosting> ScholarshipPostings => Set<ScholarshipPosting>();
    public DbSet<ScholarshipDecree> ScholarshipDecrees => Set<ScholarshipDecree>();

    #endregion Scholarships

    #region Selections

    public DbSet<SelectionOverview> SelectionOverviews => Set<SelectionOverview>();
    public DbSet<SelectionAdministration> SelectionAdministrations => Set<SelectionAdministration>();
    public DbSet<SelectionTOEFL> SelectionTOEFLs => Set<SelectionTOEFL>();
    public DbSet<SelectionTOEFLSchedule> SelectionTOEFLSchedules => Set<SelectionTOEFLSchedule>();
    public DbSet<SelectionPsychotest> SelectionPsychotests => Set<SelectionPsychotest>();
    public DbSet<SelectionPsychotestSchedule> SelectionPsychotestSchedules => Set<SelectionPsychotestSchedule>();
    public DbSet<SelectionTalentReview> SelectionTalentReviews => Set<SelectionTalentReview>();
    public DbSet<SelectionTalentReviewFirst> SelectionTalentReviewFirsts => Set<SelectionTalentReviewFirst>();
    public DbSet<SelectionTalentReviewSecond> SelectionTalentReviewSeconds => Set<SelectionTalentReviewSecond>();
    public DbSet<SelectionDecree> SelectionDecrees => Set<SelectionDecree>();
    public DbSet<SelectionDecreeHistory> SelectionDecreeHistories => Set<SelectionDecreeHistory>();
    public DbSet<SelectionBeritaAcara> SelectionBeritaAcaras => Set<SelectionBeritaAcara>();
    public DbSet<SelectionApplicantBeritaAcara> SelectionApplicantBeritaAcaras => Set<SelectionApplicantBeritaAcara>();
    public DbSet<SelectionDecreeDraft> SelectionDecreeDrafts => Set<SelectionDecreeDraft>();
    public DbSet<SelectionWarrantDraft> SelectionWarrantDrafts => Set<SelectionWarrantDraft>();
    public DbSet<SelectionDecreeDocument> SelectionDecreeDocuments => Set<SelectionDecreeDocument>();

    #endregion Selections

    #region Log

    public DbSet<HrisLog> HrisLogs => Set<HrisLog>();

    #endregion Log

    #region Study Preparation

    public DbSet<Ielts> Ieltses => Set<Ielts>();
    public DbSet<Gmat> Gmats => Set<Gmat>();
    public DbSet<Gre> Gres => Set<Gre>();
    public DbSet<Essay> Essays => Set<Essay>();
    public DbSet<SponsorshipLetter> SponsorshipLetters => Set<SponsorshipLetter>();
    public DbSet<SponsorshipLetterApproval> SponsorshipLetterApprovals => Set<SponsorshipLetterApproval>();
    public DbSet<VisaGuarantee> VisaGuarantees => Set<VisaGuarantee>();

    #endregion Study Preparation

    #region Study Monitoring

    public DbSet<StudyPeriod> StudyPeriods => Set<StudyPeriod>();
    public DbSet<Graduate> Graduates => Set<Graduate>();
    public DbSet<AfterProgram> AfterPrograms => Set<AfterProgram>();
    public DbSet<StudyMonitoringComment> StudyMonitoringComments => Set<StudyMonitoringComment>();

    #endregion Study Monitoring

    #region University Registration

    public DbSet<Recommendation> Recommendations => Set<Recommendation>();
    public DbSet<Admission> Admissions => Set<Admission>();
    public DbSet<AdmissionAgreement> AdmissionAgreements => Set<AdmissionAgreement>();
    public DbSet<AdmissionWarrant> AdmissionWarrants => Set<AdmissionWarrant>();
    public DbSet<AdmissionSuperiorLetter> AdmissionSuperiorLetters => Set<AdmissionSuperiorLetter>();

    #endregion University Registration

    #region Payment

    public DbSet<Reimbursement> Reimbursements => Set<Reimbursement>();
    public DbSet<Disbursement> Disbursements => Set<Disbursement>();
    public DbSet<ItemStudyAssignment> ItemStudyAssignments => Set<ItemStudyAssignment>();
    public DbSet<StudyAssignmentFee> StudyAssignmentFees => Set<StudyAssignmentFee>();
    public DbSet<PaymentOverview> PaymentOverviews => Set<PaymentOverview>();
    public DbSet<CategoryReimbursement> CategoryReimbursement => Set<CategoryReimbursement>();
    public DbSet<PaymentComment> PaymentComments => Set<PaymentComment>();
    #endregion Payment

    #region Innovation Project

    public DbSet<ProjectPlan> ProjectPlans => Set<ProjectPlan>();
    public DbSet<ProjectMentoring> ProjectMentorings => Set<ProjectMentoring>();
    public DbSet<ProjectCharterPlan> ProjectCharterPlans => Set<ProjectCharterPlan>();
    public DbSet<ProjectCharterRealization> ProjectCharterRealizations => Set<ProjectCharterRealization>();
    public DbSet<ProjectComment> ProjectComments => Set<ProjectComment>();

    #endregion Innovation Project

    public PsmpDbContext(
        DbContextOptions<PsmpDbContext> options,
        ICurrentUserService currentUser,
        IDateAndTimeService dateTime,
        IDomainEventService domainEvent) : base(options)
    {
        _currentUser = currentUser;
        _dateTime = dateTime;
        _domainEvent = domainEvent;
    }

    protected PsmpDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public IDbContextTransaction BeginTransaction()
    {
        return Database.BeginTransaction();
    }
    public async Task<int> SaveChangesAsync<THandler>(THandler handler, CancellationToken cancellationToken = default) where THandler : notnull
    {
        foreach (var entry in ChangeTracker.Entries<ICreatable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = _currentUser.Username;
                    entry.Entity.Created = _dateTime.Now;
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<IModifiable>())
        {
            switch (entry.State)
            {
                case EntityState.Modified:
                    entry.Entity.ModifiedBy = _currentUser.Username;
                    entry.Entity.Modified = _dateTime.Now;
                    break;
            }
        }

        var auditEntries = OnBeforeSaveChanges(handler.GetType().Name);
        var result = await base.SaveChangesAsync(true, cancellationToken);

        await OnAfterSaveChanges(auditEntries);
        await DispatchEvents();

        return result;
    }

    private List<AuditEntry> OnBeforeSaveChanges(string actionName)
    {
        ChangeTracker.DetectChanges();

        var auditEntries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries())
        {
            var isAuditableEntity = entry.Entity.GetType().IsAuditableEntity();

            if (!isAuditableEntity || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
            {
                continue;
            }

            var entityType = Model.FindEntityType(entry.Entity.GetType());

            if (entityType is null)
            {
                throw new InvalidOperationException($"Cannot find Entity Type for {entry.Entity.GetType()}");
            }

            var tableName = $"{entityType.GetSchema()}.{entityType.GetTableName()}";
            var entityName = entry.Entity.GetType().Name;

            var auditEntry = new AuditEntry
            {
                TableName = tableName,
                EntityName = entityName,
                ClientApplicationId = _currentUser.ClientId,
                CreatedBy = _currentUser.Username,
                FromIpAddress = _currentUser.IpAddress,
                FromGeolocation = _currentUser.Geolocation,
                ActionName = actionName,
                ActionType = entry.State.ToString()
            };

            auditEntries.Add(auditEntry);

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary)
                {
                    // The value will be generated by the database, get the value after saving.
                    auditEntry.TemporaryProperties.Add(property);
                    continue;
                }

                var propertyName = property.Metadata.Name;
                var originalValue = property.OriginalValue;
                var currentValue = property.CurrentValue;

                if (property.Metadata.IsPrimaryKey())
                {
                    if (currentValue is null)
                    {
                        continue;
                    }

                    if (propertyName == nameof(IHasKey.Id))
                    {
                        var entityId = currentValue.ToString();

                        if (!string.IsNullOrWhiteSpace(entityId))
                        {
                            auditEntry.EntityId = new Guid(entityId);
                        }
                    }

                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        if (propertyName is nameof(IDeletable.IsDeleted) or nameof(IModifiable.Modified) or nameof(IModifiable.ModifiedBy))
                        {
                            continue;
                        }

                        auditEntry.NewValues[propertyName] = currentValue;
                        break;

                    case EntityState.Deleted:
                        auditEntry.OldValues[propertyName] = originalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            auditEntry.OldValues[propertyName] = originalValue;
                            auditEntry.NewValues[propertyName] = currentValue;

                            if (propertyName is nameof(IDeletable.IsDeleted) && (bool)currentValue! is true)
                            {
                                auditEntry.ActionType = SoftDeleted;
                            }
                        }

                        break;
                }
            }
        }

        // Save audit entities that have all the modifications.
        foreach (var auditEntry in auditEntries.Where(x => !x.HasTemporaryProperties))
        {
            Audits.Add(auditEntry.ToAudit());
        }

        // Keep a list of entries where the value of some properties are unknown at this step.
        return auditEntries.Where(x => x.HasTemporaryProperties).ToList();
    }

    private Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
    {
        if (auditEntries is null || auditEntries.Count == 0)
        {
            return Task.CompletedTask;
        }

        foreach (var auditEntry in auditEntries)
        {
            // Get the final value of the temporary properties.
            foreach (var property in auditEntry.TemporaryProperties)
            {
                var currentValue = property.CurrentValue;
                var propertyName = property.Metadata.Name;

                if (currentValue is null)
                {
                    continue;
                }

                if (property.Metadata.IsPrimaryKey())
                {
                    if (propertyName == nameof(IHasKey.Id))
                    {
                        var entityId = currentValue.ToString();

                        if (!string.IsNullOrWhiteSpace(entityId))
                        {
                            auditEntry.EntityId = new Guid(entityId);
                        }
                    }
                }
                else
                {
                    auditEntry.NewValues[propertyName] = currentValue;
                }
            }

            Audits.Add(auditEntry.ToAudit());
        }

        return SaveChangesAsync();
    }

    private async Task DispatchEvents()
    {
        while (true)
        {
            var domainEventEntity = ChangeTracker.Entries<IHasDomainEvent>()
                .Select(x => x.Entity.DomainEvents)
                .SelectMany(x => x)
                .Where(domainEvent => !domainEvent.IsPublished)
                .FirstOrDefault();

            if (domainEventEntity is null)
            {
                break;
            }

            domainEventEntity.IsPublished = true;

            await _domainEvent.Publish(domainEventEntity);
        }
    }
}
