/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

namespace Smlp.Shared.Common.Responses;

public class CommonCreateResponse : Response
{
    public Guid Id { get; set; }
    public string? Description { get; set; }
}
