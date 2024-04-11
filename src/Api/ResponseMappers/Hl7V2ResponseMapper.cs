using Api.Responses;
using Core;
using Core.Common.Exceptions;
using Core.Ingestion.Models;
using Core.Ingestion.Utilities;
using FluentValidation.Results;

namespace Api.ResponseMappers;

public class HL7v2ResponseMapper(IngestionRequest ingestionRequest) : IResponseMapper
{
    private readonly string _messageControlId = HL7v2Utility.GetMessageControlId(ingestionRequest.Message);

    public IResult GenerateSuccessfulResult()
    {
        var response = GenerateResponseMessage("AA", _messageControlId, "Successfully processed");
        return TypedResults.Content(response, statusCode: StatusCodes.Status200OK);
    }

    public IResult MapExceptionToErrorResult(Exception exception)
    {
        return exception switch
        {
            UnsupportedTemplateException => TypedResults.Content(
                GenerateResponseMessage("AR", _messageControlId, exception.Message), statusCode: StatusCodes.Status400BadRequest),
            _ => new InternalServerError(GenerateResponseMessage("AE", _messageControlId, exception.Message))
        };
    }

    public IResult MapValidationErrorsToErrorResult(ValidationResult validationResult)
    {
        if (validationResult.Errors.Any(x => x.ErrorCode == "The message must be a valid HL7v2 message when the Data Type is HL7v2."))
        {
            return TypedResults.BadRequest(validationResult.Errors.Select(error => error.ErrorMessage).ToList());
        }

        var response = GenerateResponseMessage("AR", _messageControlId,
            validationResult.ToString());
        return TypedResults.Content(response, statusCode: StatusCodes.Status400BadRequest);
    }

    private string GenerateResponseMessage(string ackCode, string requestMessageControlId, string textMessage)
    {
        return
            $"""
             MSH|^~\&|{Globals.DataPlatformName}|{Globals.SendingFacility}|{ingestionRequest.SourceDomain}|{ingestionRequest.OrganisationCode}|{DateTime.Now:yyyyMMddHHmmss}||ACK|{Guid.NewGuid().ToString()}|P|{Globals.HL7v2Version}
             MSA|{ackCode}|{requestMessageControlId}|{textMessage}
             """;
    }
}