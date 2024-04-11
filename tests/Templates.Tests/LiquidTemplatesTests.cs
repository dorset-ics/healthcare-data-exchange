using Core.Common.Abstractions.Clients;
using Core.Common.Models;
using Core.Common.Results;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Templates.Tests.DataProviders;
using Templates.Tests.Validators;

namespace Templates.Tests;

public class LiquidTemplatesTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>, IDisposable
{
    private readonly IDataHubFhirClient _dataHubFhirClient = factory.Services.GetService<IDataHubFhirClient>()
                                                             ?? throw new Exception("Failed to resolve IDataHubFhirClient from the service provider.");

    [Theory]
    [MemberData(nameof(TestFileProvider.GetTestInputFiles), MemberType = typeof(TestFileProvider))]
    public async void TestTemplateConversionWithGivenInput(string relativePath, TemplateInfo templateInfo)
    {
        var filePath = Path.Combine(TestPaths.InputDirectoryPath, relativePath);
        var inputData = await File.ReadAllTextAsync(filePath);

        var result = await CallConvert(templateInfo, inputData);

        var bundle = await CallValidate(templateInfo, result);

        string responseContent = await new FhirJsonSerializer(new SerializerSettings()
        {
            Pretty = true
        }).SerializeToStringAsync(bundle);

        await TestResultValidator.CompareResponseWithExpectedFile(relativePath, responseContent);
        FhirResourceValidator.ValidateMetaProfileExistsInResource(bundle);
    }

    private async Task<Bundle> CallValidate(TemplateInfo templateInfo, Result<Bundle> result)
    {
        var bundle = result.Value;
        var validationResult = await _dataHubFhirClient.ValidateData(bundle);
        validationResult.IsSuccess.ShouldBeTrue($"Error validating {templateInfo.DataType} data using template {templateInfo.Name}: {validationResult.Exception?.Message}");
        return bundle;
    }

    private async Task<Result<Bundle>> CallConvert(TemplateInfo templateInfo, string inputData)
    {
        var convertRequest = new ConvertDataRequest(inputData, templateInfo);
        var result = await _dataHubFhirClient.ConvertData(convertRequest);
        result.IsSuccess.ShouldBeTrue($"Error converting {templateInfo.DataType} data using template {templateInfo.Name}: {result.Exception?.Message}");
        return result;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}