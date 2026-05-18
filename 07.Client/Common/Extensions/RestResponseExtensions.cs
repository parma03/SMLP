using System.Net;
using System.Net.Mime;
using Newtonsoft.Json;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Common.Responses;
using RestSharp;

namespace Psmp.Client.Common.Extensions;

public static class RestResponseExtensions
{
    public static ResponseResult<T> ToResponseResult<T>(this RestResponse restResponse) where T : Response
    {
        var responseResult = new ResponseResult<T>();

        try
        {
            if (restResponse.IsSuccessful)
            {
                if (typeof(T).IsSubclassOf(typeof(FileResponse)))
                {
                    if (restResponse.ContentHeaders is null)
                    {
                        throw new Exception("Response headers is null");
                    }

                    var contentDispositionContentHeader = restResponse.ContentHeaders
                            .Where(x => x.Name == "Content-Disposition")
                            .FirstOrDefault();

                    if (contentDispositionContentHeader is null)
                    {
                        throw new Exception("Content-Disposition Content Header is null");
                    }

                    if (contentDispositionContentHeader.Value is not string contentDispositionValue)
                    {
                        throw new Exception("Content-Disposition Value is null");
                    }

                    var contentDisposition = new ContentDisposition(contentDispositionValue);
                    var fileName = contentDisposition.FileName;

                    if (fileName is null)
                    {
                        throw new Exception("ContentDisposition.FileName is null");
                    }

                    if (restResponse.RawBytes is null)
                    {
                        throw new Exception("Response content is null");
                    }

                    if (restResponse.ContentType is null)
                    {
                        throw new Exception("Response MIME content type is null");
                    }

                    var response = new FileResponse
                    {
                        FileName = fileName,
                        Content = restResponse.RawBytes,
                        ContentType = restResponse.ContentType
                    };

                    var serializedDataResponse = JsonConvert.SerializeObject(response);
                    responseResult.Result = JsonConvert.DeserializeObject<T>(serializedDataResponse);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(restResponse.Content))
                    {
                        responseResult.Result = new SuccessResponse() as T;
                    }
                    else
                    {
                        var response = JsonConvert.DeserializeObject<T>(restResponse.Content);

                        if (response is null)
                        {
                            throw new Exception($"Failed to deserialize JSON content {restResponse.Content} into {typeof(T).Name}.");
                        }

                        responseResult.Result = response;
                    }
                }
            }
            else
            {
                responseResult.Error = CreateErrorResponse(restResponse);

                if (restResponse.ErrorException is not null)
                {
                    responseResult.Error.Exception = restResponse.ErrorException;
                }
            }
        }
        catch (Exception exception)
        {
            responseResult.Error = new CommonErrorResponse
            {
                Exception = exception,
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6",
                Title = $"{exception.GetType().FullName}: {exception.Message}",
                Status = restResponse.StatusCode,
                Detail = $"{restResponse.Content} [{exception.GetType().FullName}: {exception.Message}] {exception.StackTrace}",
            };
        }

        return responseResult;
    }

    private static ErrorResponse CreateErrorResponse(RestResponse restResponse)
    {
        if (restResponse.StatusCode == 0 && restResponse.ErrorException is WebException)
        {
            return new ServiceUnavailableResponse
            {
                Title = HttpStatusCode.ServiceUnavailable.ToString().SplitWords(),
                Status = HttpStatusCode.ServiceUnavailable,
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.4",
                Detail = $"Service at {restResponse.ResponseUri} is not available. Error message: {restResponse.ErrorMessage}"
            };
        }

        if (string.IsNullOrWhiteSpace(restResponse.Content))
        {
            if (restResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return new CommonErrorResponse
                {
                    Title = "The specified resource was not found",
                    Status = HttpStatusCode.NotFound,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Detail = $"The specified resource at {restResponse.ResponseUri} was not found."
                };
            }
            else if (restResponse.StatusCode == HttpStatusCode.UnsupportedMediaType)
            {
                return new CommonErrorResponse
                {
                    Title = HttpStatusCode.UnsupportedMediaType.ToString().SplitWords(),
                    Status = HttpStatusCode.UnsupportedMediaType,
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.13",
                    Detail = $"The server is refusing to accept the request because the media type is not supported."
                };
            }
            else if (restResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new CommonErrorResponse
                {
                    Title = "The requested resource requires authentication.",
                    Status = HttpStatusCode.Unauthorized,
                    Type = "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
                    Detail = $"Please reload the page and try again."
                };
            }
            else if (restResponse.StatusCode == 0)
            {
                return new CommonErrorResponse
                {
                    Title = "Unreachable Server",
                    Status = restResponse.StatusCode,
                    Type = "The server might be down",
                    Detail = restResponse.ErrorMessage!
                };
            }
            else
            {
                return new CommonErrorResponse
                {
                    Title = "Unknown Error",
                    Status = HttpStatusCode.InternalServerError,
                    Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
                    Detail = "Something went wrong."
                };
            }
        }
        else
        {
            if (restResponse.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorResponse = JsonConvert.DeserializeObject<BadRequestResponse>(restResponse.Content);

                if (errorResponse is null)
                {
                    throw new Exception($"Failed to deserialize JSON content {restResponse.Content} into {nameof(BadRequestResponse)}.");
                }

                return errorResponse;
            }
            else
            {
                var errorResponse = JsonConvert.DeserializeObject<CommonErrorResponse>(restResponse.Content);

                if (errorResponse is null)
                {
                    throw new Exception($"Failed to deserialize JSON content {restResponse.Content} into {nameof(CommonErrorResponse)}.");
                }

                return errorResponse;
            }
        }
    }
}
