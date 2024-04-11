using FluentValidation;
using Hl7.Fhir.Model;

namespace Core.Common.Validators;

public class ValidateOperationOutcomeValidator : AbstractValidator<OperationOutcome>
{
    private const string allOk = "All OK";

    public ValidateOperationOutcomeValidator()
    {

        RuleFor(x => x.Issue)
            .Cascade(CascadeMode.Continue)
            .Must(x => x.All(issue =>
                issue.Severity is OperationOutcome.IssueSeverity.Success or OperationOutcome.IssueSeverity.Information
                    or OperationOutcome.IssueSeverity.Warning))
            .WithMessage("OperationOutcome must have Success or Information severity.")
            .Must(x => x.All(issue
                => issue.Diagnostics is not null && issue.Diagnostics == allOk
                   || issue.Diagnostics is null))
            .WithMessage("OperationOutcome must have all OK diagnostics or no diagnostics at all.");
    }
}