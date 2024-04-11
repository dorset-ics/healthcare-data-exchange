using Core.Common.Abstractions.Clients;
using Core.Common.Models;
using Core.Common.Strategies;
using Core.Ingestion.Abstractions;
using Core.Ingestion.Enums;
using Core.Ingestion.Models;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace Unit.Tests.Core.Ingestion.Strategies
{
    public class HL7v2IngestionStrategyTests
    {
        private const string SourceDomain = "domain";
        private const string OrganisationCode = "org";
        private readonly IDataHubFhirClient _mockDataHubClient;
        private readonly HL7v2IngestionStrategy _strategyUnderTest;
        private readonly string _message;

        public HL7v2IngestionStrategyTests()
        {
            _mockDataHubClient = Substitute.For<IDataHubFhirClient>();
            var mockLogger = Substitute.For<ILogger<HL7v2IngestionStrategy>>();
            var _fhirResourceEnhancer = Substitute.For<IFhirResourceEnhancer>();
            _strategyUnderTest = new HL7v2IngestionStrategy(_mockDataHubClient, _fhirResourceEnhancer, mockLogger);
            _message = "||||||||adt^a01^adtasd|||||||";
        }

        [Fact]
        public async Task GivenValidIngestionRequest_WhenIngestIsCalled_ThenShouldReturnSuccessResult()
        {
            _mockDataHubClient.ConvertData(Arg.Any<ConvertDataRequest>()).Returns(new Bundle());
            _mockDataHubClient.ValidateData(Arg.Any<Bundle>()).Returns(new OperationOutcome());
            _mockDataHubClient.TransactionAsync<Bundle>(Arg.Any<Bundle>()).Returns(new Bundle());

            var result = await _strategyUnderTest.Ingest(new IngestionRequest(OrganisationCode, SourceDomain, IngestionDataType.HL7v2, _message)).ConfigureAwait(true);

            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task GivenIngestionRequest_WhenConversionFails_ThenShouldReturnFailureResult()
        {
            _mockDataHubClient.ConvertData(Arg.Any<ConvertDataRequest>()).Returns(x => new Exception("Conversion Failed"));
            _mockDataHubClient.ValidateData(Arg.Any<Bundle>()).Returns(new OperationOutcome());
            _mockDataHubClient.TransactionAsync<Bundle>(Arg.Any<Bundle>()).Returns(new Bundle());

            var result = await _strategyUnderTest.Ingest(new IngestionRequest(OrganisationCode, SourceDomain, IngestionDataType.HL7v2, _message)).ConfigureAwait(true);

            result.IsFailure.ShouldBeTrue();
            result.Exception.Message.ShouldBe("Conversion Failed");
        }

        [Fact]
        public async Task GivenIngestionRequest_WhenValidationFails_ThenShouldReturnFailureResult()
        {
            _mockDataHubClient.ConvertData(Arg.Any<ConvertDataRequest>()).Returns(new Bundle());
            _mockDataHubClient.ValidateData(Arg.Any<Bundle>()).Returns(x => new Exception("Validation Failed"));
            _mockDataHubClient.TransactionAsync<Bundle>(Arg.Any<Bundle>()).Returns(new Bundle());

            var result = await _strategyUnderTest.Ingest(new IngestionRequest(OrganisationCode, SourceDomain, IngestionDataType.HL7v2, _message)).ConfigureAwait(true);

            result.IsFailure.ShouldBeTrue();
            result.Exception.Message.ShouldBe("Validation Failed");
        }

        [Fact]
        public async Task GivenIngestionRequest_WhenTransactionFails_ThenShouldReturnFailureResult()
        {
            _mockDataHubClient.ConvertData(Arg.Any<ConvertDataRequest>()).Returns(new Bundle());
            _mockDataHubClient.ValidateData(Arg.Any<Bundle>()).Returns(new OperationOutcome());
            _mockDataHubClient.TransactionAsync<Bundle>(Arg.Any<Bundle>()).Returns(x => new Exception("Transaction Failed"));

            var result = await _strategyUnderTest.Ingest(new IngestionRequest(OrganisationCode, SourceDomain, IngestionDataType.HL7v2, _message)).ConfigureAwait(true);

            result.IsFailure.ShouldBeTrue();
            result.Exception.Message.ShouldBe("Transaction Failed");
        }
    }
}