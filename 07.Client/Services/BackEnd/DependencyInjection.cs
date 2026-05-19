/*
*/

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Smlp.Client.Services.BackEnd.Applicants;
using Smlp.Client.Services.BackEnd.Masters;
using Smlp.Client.Services.BackEnd.Payments;
using Smlp.Client.Services.BackEnd.Scholarships;
using Smlp.Client.Services.BackEnd.Selections;
using Smlp.Client.Services.BackEnd.SiteAdministrators;
using Smlp.Client.Services.BackEnd.StudyMonitorings;
using Smlp.Client.Services.BackEnd.StudyPreparations;
using Smlp.Client.Services.BackEnd.Universities;

namespace Smlp.Client.Services.BackEnd;

public static class DependencyInjection
{
    public static IServiceCollection AddBackEndService(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration != null)
        {
            services.Configure<BackEndOptions>(configuration.GetSection(BackEndOptions.SectionKey));
        }

        #region Essential Services

        services.AddTransient<AuditService>();
        services.AddTransient<CompanyGroupService>();
        services.AddTransient<CompanyService>();
        services.AddTransient<DirectorateService>();
        services.AddTransient<DivisionService>();
        services.AddTransient<FungsiService>();
        services.AddTransient<DegreeService>();
        services.AddTransient<FacultyService>();
        services.AddTransient<ScholarshipSubjectService>();
        services.AddTransient<UsersService>();

        #endregion Essential Services

        #region Business Services

        services.AddTransient<DocumentService>();
        services.AddTransient<SelectionAdministrationService>();

        #endregion Business Services

        #region Applicant

        services.AddTransient<RegistrationService>();
        services.AddTransient<ApplicantService>();
        services.AddTransient<ApplicantScheduleService>();
        services.AddTransient<ApplicantByPassService>();
        services.AddTransient<ApplicantStudyPreparationService>();

        #endregion Applicant

        #region Scholarships

        services.AddTransient<ScholarshipPlanService>();
        services.AddTransient<ScholarshipPostingService>();
        services.AddTransient<ScholarshipPostingApplicantService>();

        #endregion Scholarships

        #region Selections

        services.AddTransient<SelectionAdministrationService>();
        services.AddTransient<SelectionTalentReviewFirstService>();
        services.AddTransient<SelectionOverviewService>();
        services.AddTransient<SelectionTOEFLService>();
        services.AddTransient<SelectionPsychotestService>();
        services.AddTransient<SelectionTalentReviewService>();
        services.AddTransient<SelectionDecreesService>();
        services.AddTransient<SelectionBeritaAcaraService>();
        #endregion Selections

        #region Study Preparation
        services.AddTransient<StudyPreparationSponsorshipLetterService>();
        services.AddTransient<StudyPreparationIeltsService>();
        services.AddTransient<StudyPreparationGmatService>();
        services.AddTransient<StudyPreparationEssayService>();
        services.AddTransient<StudyPreparationGreService>();
        services.AddTransient<StudyPreparationVisaGuaranteeService>();
        #endregion

        #region Study Monitorings
        services.AddTransient<StudyMonitoringStudyPeriodService>();
        services.AddTransient<StudyMonitoringGraduateService>();
        services.AddTransient<StudyMonitoringAfterProgramService>();
        services.AddTransient<StudyMonitoringCommentService>();
        #endregion

        #region Payments
        services.AddTransient<PaymentService>();
        services.AddTransient<StudyAssignmentService>();
        services.AddTransient<DisbursementService>();
        services.AddTransient<SetDataStatusService>();
        #endregion

        #region Universities
        services.AddTransient<UniversityAdmissionService>();
        services.AddTransient<UniversityAdmissionSuperiorLetterService>();
        services.AddTransient<UniversityAdmissionAgreementService>();
        services.AddTransient<UniversityAdmissionWarrantService>();
        services.AddTransient<UniversityRecommendationService>();
        #endregion

        #region Emails

        services.AddTransient<EmailService>();

        #endregion Emails

        #region Project
        services.AddTransient<Smlp.Client.Services.BackEnd.Projects.ProjectPlansService>();
        services.AddTransient<Smlp.Client.Services.BackEnd.Projects.ProjectCharterPlansService>();
        services.AddTransient<Smlp.Client.Services.BackEnd.Projects.ProjectCharterRealisationService>();
        services.AddTransient<Smlp.Client.Services.BackEnd.Projects.ProjectMonitoringService>();
        #endregion

        #region Site Administrator
        services.AddTransient<MentorService>();
        services.AddTransient<BuddyService>();
        #endregion

        return services;
    }
}
