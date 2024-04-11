using Core.Ingestion.Enums;
using Core.Ingestion.Models;
using FluentValidation;

namespace Core.Ingestion.Validators;

public class IngestionRequestValidator : AbstractValidator<IngestionRequest>
{
    public IngestionRequestValidator()
    {
        RuleFor(x => x.OrganisationCode)
            .NotNull().WithMessage("Organisation code cannot be null when provided.")
            .NotEmpty().WithMessage("Organisation code cannot be empty or whitespaces when provided.");

        RuleFor(x => x.SourceDomain)
            .NotNull().WithMessage("Source domain cannot be null when provided.")
            .NotEmpty().WithMessage("Source domain cannot be empty or whitespaces when provided.");

        RuleFor(x => x.IngestionDataType)
            .IsInEnum().WithMessage("Data Type must be a valid Ingestion Data Type.");

        RuleFor(x => x.Message)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("The hl7 message cannot be null when ingestion data type is hl7v2.")
            .When(x => x.IngestionDataType == IngestionDataType.HL7v2)
            .NotEmpty().WithMessage("The hl7 message cannot be empty when provided.")
            .When(x => x.IngestionDataType == IngestionDataType.HL7v2)
            .Must(HL7v2DataValidator.ValidateMessageHeader)
            .When(x => x.IngestionDataType == IngestionDataType.HL7v2)
            .WithMessage("The message must be a valid HL7v2 message when the Data Type is HL7v2.");
    }
}