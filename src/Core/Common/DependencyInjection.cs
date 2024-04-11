using Core.Common.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Common;

public static class DependencyInjection
{
    public static IServiceCollection AddCommon(this IServiceCollection services)
        => services
            .AddFluentValidators();

    private static IServiceCollection AddFluentValidators(this IServiceCollection services)
        => services
            .AddValidatorsFromAssemblyContaining<ValidateOperationOutcomeValidator>()
            .AddValidatorsFromAssemblyContaining<NhsNumberValidator>();
}