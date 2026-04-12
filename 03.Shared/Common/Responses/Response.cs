/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

namespace Smlp.Shared.Common.Responses;

public abstract class Response
{
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
}
