using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Text;

namespace Api.Responses;

sealed class InternalServerError(string value) : IResult, IValueHttpResult<string>
{
    public string? Value { get; } = value;

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = MediaTypeNames.Text.Plain;
        httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(Value ?? string.Empty);
        await httpContext.Response.WriteAsync(Value ?? string.Empty);
    }
}