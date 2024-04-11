using System.Net;
using Api.ResponseMappers;
using Core.Common.Exceptions;
using Core.Ingestion.Enums;
using Core.Ingestion.Models;
using Core.Ingestion.Validators;
using FluentValidation.Results;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Http;
using Unit.Tests.Api.Utilities;

namespace Unit.Tests.Api.ResponseMappers;

public class HL7v2ResponseMapperTest
{
    private readonly IngestionRequestValidator _validator = new();

    private readonly HL7v2ResponseMapper _sut = new(new IngestionRequest
    (
        OrganisationCode: "org",
        SourceDomain: "domain",
        IngestionDataType: IngestionDataType.HL7v2,
        Message: @"MSH|^~\\&|AGYLEED|R0D01|INTEGRATION-ENGINE|RDZ|20231025104649||ADT^A28|some-string|P|2.4|||AL|NE"
    ));

    [Fact]
    public void GenerateSuccessfulResult_ShouldReturnCorrectResponse_WhenIngestionDataTypeIsHL7V2()
    {
        const string msgControlId = "some-string";
        const string expectedAck = $"MSA|AA|{msgControlId}|Successfully processed";

        var actualResponse = _sut.GenerateSuccessfulResult();
        actualResponse.ShouldBeStatusWithAckMessage(expectedAck, StatusCodes.Status200OK);
    }

    [Fact]
    public void MapExceptionToErrorResult_GivenFhirOperationException_()
    {
        const string msgControlId = "some-string";
        const string errorMessage = "some-error";
        const string expectedHl7BadRequestException = $"MSA|AE|{msgControlId}|{errorMessage}";

        var hl7BadRequestException = new FhirOperationException(errorMessage, HttpStatusCode.BadRequest);

        var actualHl7BadRequestException = _sut.MapExceptionToErrorResult(hl7BadRequestException);
        actualHl7BadRequestException.ShouldBeInternalServerErrorWithNackMessage(expectedHl7BadRequestException);
    }

    [Fact]
    public void MapExceptionToErrorResult_ShouldReturnInternalErrorException_WhenHL7IngestionFails()
    {
        const string msgControlId = "some-string";
        const string errorMessage = "some-error";
        const string expectedInternalServerErrorException = $"MSA|AE|{msgControlId}|{errorMessage}";

        var internalServerErrorException = new Exception(errorMessage);

        var actualHl7InternalServerError = _sut.MapExceptionToErrorResult(internalServerErrorException);
        actualHl7InternalServerError.ShouldBeInternalServerErrorWithNackMessage(expectedInternalServerErrorException);
    }

    [Fact]
    public void ShouldMapValidationErrorsToErrorResultWhenOrgIsMissingAndHl7IngestionDataType()
    {
        var invalidInput = new IngestionRequest
        (
            OrganisationCode: "org",
            SourceDomain: "domain",
            IngestionDataType: (IngestionDataType)999,
            Message: @"MSH|^~\\&|AGYLEED|R0D01|INTEGRATION-ENGINE|RDZ|20231025104649||ADT^A28|some-string|P|2.4|||AL|NE"
        );

        const string msgControlId = "some-string";
        const string errorMessage = "Data Type must be a valid Ingestion Data Type.";
        const string expectedHl7BadRequestException = $"MSA|AR|{msgControlId}|{errorMessage}";
        var responseMapper = new HL7v2ResponseMapper(invalidInput);

        var validationResult = _validator.Validate(invalidInput);

        var result = responseMapper.MapValidationErrorsToErrorResult(validationResult);
        result.ShouldBeStatusWithAckMessage(expectedHl7BadRequestException, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void GivenUnsupportedTemplateException_WhenMapExceptionToErrorResultIsCalled_ThenBadRequestIsReturned()
    {
        var exception = new UnsupportedTemplateException("Test exception");

        var result = _sut.MapExceptionToErrorResult(exception);

        result.ShouldBeStatusWithAckMessage("MSA|AR|some-string|Test exception", StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void GivenValidationResultWithSpecificErrorCode_WhenMapValidationErrorsToErrorResultIsCalled_ThenBadRequestIsReturned()
    {
        var validationResult = new ValidationResult(new List<ValidationFailure>
        {
            new("TestProperty", "Test error message") { ErrorCode = "The message must be a valid HL7v2 message when the Data Type is HL7v2." }
        });

        var result = _sut.MapValidationErrorsToErrorResult(validationResult);
        result.ShouldBeBadRequestWithValue(new List<string> { "Test error message" });
    }
}