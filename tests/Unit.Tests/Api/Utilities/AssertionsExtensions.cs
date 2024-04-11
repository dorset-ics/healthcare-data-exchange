using System.Text.RegularExpressions;
using Api.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Unit.Tests.Api.Utilities;

public static partial class AssertionsExtensions
{
    public static void ShouldBeOkWithValue<TValue>(this IResult result, TValue expected)
    {
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Ok<TValue>>();
        var okResult = result as Ok<TValue>;
        okResult?.Value.ShouldBe(expected);
    }

    public static void ShouldBeBadRequestWithValue<TValue>(this IResult result, TValue expected) where TValue : class
    {
        result.ShouldNotBeNull();
        result.ShouldBeOfType<BadRequest<TValue>>();
        var badRequest = result as BadRequest<TValue>;
        badRequest.ShouldNotBeNull();
        badRequest.Value.ShouldNotBeNull();
        badRequest.Value.ShouldBe(expected);
    }

    public static void ShouldBeOk<TValue>(this IResult result)
    {
        result.ShouldNotBeNull();
        result.ShouldBeOfType<Ok<TValue>>();
        var okResult = result as Ok<TValue>;
        okResult.ShouldNotBeNull();
    }
    public static void ShouldBeStatusWithAckMessage(this IResult result, string expectedAck, int expectedStatusCode)
    {
        var responseResult = ShouldBeStatusWithNotNullContent(result, expectedStatusCode);
        responseResult.ResponseContent!.ShouldHaveExpectedAck(expectedAck);
    }
    public static void ShouldBeStatusWithMessage(this IResult result, string expectedMessage, int expectedStatusCode)
    {
        var responseResult = ShouldBeStatusWithNotNullContent(result, expectedStatusCode);
        responseResult.ResponseContent.ShouldBe(expectedMessage);
    }
    private static ContentHttpResult ShouldBeStatusWithNotNullContent(IResult result, int expectedStatusCode)
    {
        result.ShouldNotBeNull();
        result.ShouldBeOfType<ContentHttpResult>();
        var responseResult = result as ContentHttpResult;
        responseResult.ShouldNotBeNull();
        responseResult.StatusCode.ShouldBe(expectedStatusCode);
        responseResult.ResponseContent.ShouldNotBeNull();
        return responseResult;
    }

    public static void ShouldBeInternalServerErrorWithNackMessage(this IResult result, string expectedAck)
    {
        result.ShouldNotBeNull();
        result.ShouldBeOfType<InternalServerError>();
        var internalServerErrorResult = result as InternalServerError;
        internalServerErrorResult.ShouldNotBeNull();
        internalServerErrorResult.Value.ShouldNotBeNull();
        internalServerErrorResult.Value.ShouldHaveExpectedAck(expectedAck);
    }

    private static void ShouldHaveExpectedAck(this string value, string expectedAck)
    {
        var messageHeader = value?.Split('\n')[0].Trim();
        var messageAck = value?.Split('\n')[1].Trim();

        messageAck.ShouldNotBeNull();
        messageHeader.ShouldNotBeNull();

        var messageHeaderFields = messageHeader.Split('|');

        messageHeaderFields[0].ShouldBe("MSH");
        messageHeaderFields[1].ShouldBe("^~\\&");
        messageHeaderFields[2].ShouldBe("DEX");
        messageHeaderFields[3].ShouldBe("QVV");
        messageHeaderFields[4].ShouldBe("domain");
        messageHeaderFields[5].ShouldBe("org");
        messageHeaderFields[8].ShouldBe("ACK");
        messageHeaderFields[11].ShouldBe("2.4");

        messageAck.ShouldBe(expectedAck);
    }

    [GeneratedRegex(HL7v2Regex.HL7v2MessageHeaderPattern)]
    private static partial Regex HL7v2MessageHeaderRegex();
}