using Core.Pds.Models;
using FluentValidation;

namespace Core.Pds.Validators;

public class PdsSearchParametersValidator : AbstractValidator<PdsSearchParameters>
{
    public PdsSearchParametersValidator()
    {
        ApplyGenericValidationRules();

        RuleFor(x => x)
            .Cascade(CascadeMode.Stop)
            .Must(HasAtLeastOneParameter)
            .WithMessage("At least one parameter must be provided.");
    }

    private void ApplyGenericValidationRules()
    {
        foreach (var property in PdsSearchParameters.GetFhirStandardPropertyInfos())
        {
            RuleFor(x => property.GetValue(x, null))
                .NotEmpty().WithMessage($"{property.Name} cannot be empty or whitespaces.")
                .When(request => property.GetValue(request, null) != null);
        }
    }

    private static bool HasAtLeastOneParameter(PdsSearchParameters request)
    {
        return typeof(PdsSearchParameters)
            .GetProperties()
            .Any(property => property.GetValue(request) != null);
    }
}