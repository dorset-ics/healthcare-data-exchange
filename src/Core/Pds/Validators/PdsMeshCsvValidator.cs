using FluentValidation;

namespace Core.Pds.Validators;

public class PdsMeshCsvValidator : AbstractValidator<string>
{
    public PdsMeshCsvValidator()
    {
        RuleFor(x => x)
            .NotEmpty().WithMessage("CSV content cannot be empty.")
            .Must(HaveValidHeader).WithMessage("CSV must be in the right format.");
    }

    private bool HaveValidHeader(string csvContent)
    {
        var lines = csvContent.Split('\n');
        return lines.Length >= 2;
    }
}