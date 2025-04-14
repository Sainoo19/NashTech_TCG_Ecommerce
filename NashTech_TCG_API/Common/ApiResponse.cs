using System.Net;

namespace NashTech_TCG_API.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; }

        public ApiResponse()
        {
            Success = true;
            Message = string.Empty;
            Errors = new List<string>();
        }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Operation completed successfully")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                StatusCode = (int)HttpStatusCode.OK,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResponse(string message, int statusCode = (int)HttpStatusCode.BadRequest)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = statusCode,
                Data = default
            };
        }

        public static ApiResponse<T> ErrorResponse(List<string> errors, string message = "One or more errors occurred", int statusCode = (int)HttpStatusCode.BadRequest)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = statusCode,
                Errors = errors,
                Data = default
            };
        }
    }
}
