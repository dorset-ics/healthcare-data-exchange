using FluentValidation.Results;

namespace Api.ResponseMappers;

public interface IResponseMapper
{
    IResult GenerateSuccessfulResult();
    IResult MapExceptionToErrorResult(Exception exception);
    IResult MapValidationErrorsToErrorResult(ValidationResult validationResult);
}