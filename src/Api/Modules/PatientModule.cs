using Api.Extensions.Patient;
using Api.Models.Patient;
using Carter;
using Core.Common.Models;
using Core.Pds.Abstractions;
using Core.Pds.Exceptions;
using Core.Pds.Models;
using FluentValidation;

namespace Api.Modules;

public class PatientModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var patientsGroup = app.MapGroup("Patient");
        patientsGroup.MapGet("/", Search).WithName("GetPatients");
        patientsGroup.MapGet("/{id}", GetPatientById).WithName("GetPatientById");
        patientsGroup.MapDelete("/{id}", DeletePatientById).WithName("DeletePatientById");
    }

    private static async Task<IResult> DeletePatientById(string id, IValidator<NhsNumber> validator, IPdsService pdsService, ILogger<PatientModule> logger)
    {
        id = id.Replace(" ", string.Empty);

        var validationResult = await validator.ValidateAsync(new NhsNumber(id));
        if (!validationResult.IsValid)
            return TypedResults.BadRequest(validationResult.Errors.Select(error => error.ErrorMessage).ToList());

        var deletePatientResult = await pdsService.DeletePatientById(id);

        return deletePatientResult.Match(
            onSuccess: TypedResults.Ok,
            onFailure: PatientNotFoundExceptionToResult
        );
    }

    private static async Task<IResult> GetPatientById(string id, IValidator<NhsNumber> validator, IPdsService pdsService, ILogger<PatientModule> logger)
    {
        id = id.Replace(" ", string.Empty);

        var validationResult = await validator.ValidateAsync(new NhsNumber(id));
        if (!validationResult.IsValid)
            return TypedResults.BadRequest(validationResult.Errors.Select(error => error.ErrorMessage).ToList());

        var getPatientResult = await pdsService.GetPatientById(id);

        return getPatientResult.Match(
            onSuccess: TypedResults.Ok,
            onFailure: PatientNotFoundExceptionToResult
        );
    }

    private static async Task<IResult> Search([AsParameters] SearchModel model, IPdsService pdsService,
        IValidator<PdsSearchParameters> validator,
        ILogger<PatientModule> logger)
    {
        var pdsSearchParameters = model.ToPdsSearchParameters();

        var validationResult = await validator.ValidateAsync(pdsSearchParameters);
        if (!validationResult.IsValid)
            return TypedResults.BadRequest(validationResult.Errors.Select(error => error.ErrorMessage).ToList());

        var pdsSearchResult = await pdsService.Search(pdsSearchParameters);

        return pdsSearchResult.Match(
            onSuccess: TypedResults.Ok,
            onFailure: PatientNotFoundExceptionToResult
        );
    }

    private static IResult PatientNotFoundExceptionToResult(Exception exception)
    {
        throw exception;
    }
}
