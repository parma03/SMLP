/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

namespace Psmp.Bsui.Features.Applicants.Selection.Models;

public class ScheduleSessionModel
{
    public Guid SelectionTOEFLScheduleId { get; set; }
    public string SessionName { get; set; } = string.Empty;
}
