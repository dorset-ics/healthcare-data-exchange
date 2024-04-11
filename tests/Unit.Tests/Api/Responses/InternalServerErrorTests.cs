using System.Net.Mime;
using System.Text;
using Api.Responses;
using Microsoft.AspNetCore.Http;

namespace Unit.Tests.Api.Responses;

public class InternalServerErrorTests
{

    [Fact]
    public async Task ExecuteAsync_ShouldEditTheResponseHttpContextWithError()
    {
        var value = "value";
        var httpContext = new DefaultHttpContext();

        await new InternalServerError(value).ExecuteAsync(httpContext);

        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        httpContext.Response.ContentType.ShouldBe(MediaTypeNames.Text.Plain);
        httpContext.Response.ContentLength.ShouldBe(Encoding.UTF8.GetByteCount(value));
    }
}