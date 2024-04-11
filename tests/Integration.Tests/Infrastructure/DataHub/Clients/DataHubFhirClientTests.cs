using Core.Common.Abstractions.Clients;
using Core.Common.Exceptions;
using Hl7.Fhir.Model;
using Microsoft.Extensions.DependencyInjection;
using Task = System.Threading.Tasks.Task;

namespace Integration.Tests.Infrastructure.DataHub.Clients;

public class DataHubFhirClientTests : IDisposable
{
    private readonly ApiWebApplicationFactory _webApplicationFactory;
    private readonly IDataHubFhirClient _fhirClient;

    public DataHubFhirClientTests()
    {
        _webApplicationFactory = new ApiWebApplicationFactory();
        _fhirClient = _webApplicationFactory.Services.GetService<IDataHubFhirClient>()
                        ?? throw new Exception("Failed to resolve IDataHubFhirClient from the service provider");
    }
    public void Dispose()
    {
        _webApplicationFactory.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GivenValidBundle_WhenValidate__ThenResultIsSuccessful()
    {
        var bundle = new Bundle
        {
            Type = Bundle.BundleType.Collection,
            Entry =
            [
                new Bundle.EntryComponent { Resource = new Patient { IdElement = new Id("patient1234") } },
                new Bundle.EntryComponent { Resource = new Organization { IdElement = new Id("org1234"), Name = "Example Org" } }
            ]
        };
        var actualOperationOutcome = await _fhirClient.ValidateData(bundle);

        var expectedOperationOutcome = new OperationOutcome
        {
            Issue =
            [
                new OperationOutcome.IssueComponent
                {
                    Severity = OperationOutcome.IssueSeverity.Information,
                    Code = OperationOutcome.IssueType.Informational,
                    Diagnostics = "All OK"
                }
            ]
        };
        actualOperationOutcome.IsSuccess.ShouldBeTrue();
        actualOperationOutcome.Exception.ShouldBeNull();
        actualOperationOutcome.Value.IsExactly(expectedOperationOutcome);
    }

    [Fact]
    public async Task GivenInvalidBundle_WhenValidate__ThenResultIsFailure()
    {
        var bundle = new Bundle
        {
            Type = Bundle.BundleType.Searchset,
            Entry =
            [
                new Bundle.EntryComponent { Resource = new Patient { IdElement = new Id("patient123") } },
                new Bundle.EntryComponent { Resource = new Organization { IdElement = new Id("org1234") } }
            ]
        };
        var actualOperationOutcome = await _fhirClient.ValidateData(bundle);

        var expectedOperationOutcome = new OperationOutcome
        {
            Issue =
            [
                new OperationOutcome.IssueComponent
                {
                    Severity = OperationOutcome.IssueSeverity.Error,
                    Code = OperationOutcome.IssueType.Invalid,
                    Diagnostics =
                        "Error occurred when parsing model: 'Type checking the data: Encountered unknown element 'name ' at location 'Resource.entry[1].resource[0].name [0]' while parsing'.",
                }
            ]
        };
        actualOperationOutcome.IsFailure.ShouldBeTrue();
        actualOperationOutcome.Exception.ShouldBeOfType<ValidationOperationOutcomeException>();
        (actualOperationOutcome.Exception as ValidationOperationOutcomeException)?.OperationOutcome.IsExactly(expectedOperationOutcome);
        actualOperationOutcome.Value.ShouldBeNull();
    }

    [Fact]
    public async Task GivenValidResource_WhenValidate_ThenResultIsSuccess()
    {
        var patient = new Patient { IdElement = new Id("123") };

        var actualOperationOutcome = await _fhirClient.ValidateData(patient);

        var expectedOperationOutcome = new OperationOutcome
        {
            Issue =
            [
                new OperationOutcome.IssueComponent
                {
                    Severity = OperationOutcome.IssueSeverity.Information,
                    Code = OperationOutcome.IssueType.Informational,
                    Diagnostics = "All OK"
                }
            ]
        };
        actualOperationOutcome.IsSuccess.ShouldBeTrue();
        actualOperationOutcome.Exception.ShouldBeNull();
        actualOperationOutcome.Value.IsExactly(expectedOperationOutcome);
    }

    [Fact]
    public async Task GivenInvalidResource_WhenValidate_ThenResultIsFailure()
    {
        var patient = new Patient();

        var actualOperationOutcome = await _fhirClient.ValidateData(patient);

        var expectedOperationOutcome = new OperationOutcome
        {
            Issue = new List<OperationOutcome.IssueComponent>
            {
                new OperationOutcome.IssueComponent
                {
                    Severity = OperationOutcome.IssueSeverity.Error, Code = OperationOutcome.IssueType.Invalid, Diagnostics = null,
                     Details = new CodeableConcept
                    {
                        Coding = new List<Coding>
                        {
                            new Coding
                            {
                                System = "http://hl7.org/fhir/dotnet-api-operation-outcome",
                                Code = "1000"
                            }
                        },
                        Text = "Element must not be empty"
                    },
                    Expression = new List<string>
                    {
                        "Patient"
                    }
                }
            }
        };
        actualOperationOutcome.IsFailure.ShouldBeTrue();
        actualOperationOutcome.Exception.ShouldBeOfType<ValidationOperationOutcomeException>();
        (actualOperationOutcome.Exception as ValidationOperationOutcomeException)?.OperationOutcome.IsExactly(expectedOperationOutcome);
        actualOperationOutcome.Value.ShouldBeNull();
    }

    [Fact]
    public async Task GivenBundleIsNull_ThenTransactionResultIsFailure()
    {
        var actualOperationOutcome = await _fhirClient.TransactionAsync<Bundle>(null!);
        actualOperationOutcome.IsFailure.ShouldBeTrue();
        actualOperationOutcome.Exception.ShouldBeOfType<ArgumentNullException>();
        actualOperationOutcome.Value.ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task GivenIdentifierIsNullOrWhitespace_ThenSearchResultIsFailure(string? identifier)
    {
        var actualOperationOutcome = await _fhirClient.SearchResourceByIdentifier<Patient>(identifier!);
        actualOperationOutcome.IsFailure.ShouldBeTrue();
        actualOperationOutcome.Exception.ShouldBeOfType<ArgumentNullException>();
        actualOperationOutcome.Value.ShouldBeNull();
    }

    [Fact]
    public async Task GivenResourceIsNull_ThenValidateResultIsFailure()
    {
        var actualOperationOutcome = await _fhirClient.ValidateData<Patient>(null);
        actualOperationOutcome.IsFailure.ShouldBeTrue();
        actualOperationOutcome.Exception.ShouldBeOfType<ArgumentNullException>();
        actualOperationOutcome.Value.ShouldBeNull();
    }

}