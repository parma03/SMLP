/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

namespace Smlp.Shared.Common.Responses;

public class ResponseResult<T> where T : Response
{
    public T? Result { get; set; }
    public ErrorResponse? Error { get; set; }
}
