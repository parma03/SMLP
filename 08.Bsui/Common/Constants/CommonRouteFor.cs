/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

namespace Psmp.Bsui.Common.Constants;

public static class CommonRouteFor
{
    public static readonly string Host = "/_Host";
    public static readonly string Index = "";
    public static readonly string About = nameof(About);
    public static readonly string MySession = nameof(MySession);
    public static readonly string Profile = nameof(Profile);

    public static readonly string Dashboard = nameof(Dashboard);
    public static readonly string Scholarships = nameof(Scholarships);
    public static readonly string Selection = nameof(Selection);
    public static readonly string SelectionClose = $"{nameof(Selection)}/Close";
    public static readonly string Registration = nameof(Registration);
    public static readonly string ApplicantHome = nameof(ApplicantHome);
    public static readonly string SiteAdministrator = nameof(SiteAdministrator);

    public static readonly string StudyPreparation = nameof(StudyPreparation);
    public static readonly string UniversityRegistration = nameof(UniversityRegistration);
    public static readonly string UnivRegistration = nameof(UnivRegistration);
    public static readonly string StudyMonitoring = nameof(StudyMonitoring);
    public static readonly string Payment = nameof(Payment);
    public static readonly string Project = nameof(Project);
    public static readonly string ProjectApplicant = nameof(ProjectApplicant);

    #region Scholarship

    public static readonly string ScholarshipCreate = nameof(ScholarshipCreate);
    public static readonly string ScholarshipDetail = nameof(ScholarshipDetail);
    public static readonly string ScholarshipUpdate = nameof(ScholarshipUpdate);

    #endregion Scholarship
}
