using Core.Common.Results;
using Core.Ingestion;
using Core.Ingestion.Abstractions;
using Core.Ingestion.Enums;
using Core.Ingestion.Models;

namespace Unit.Tests.Core.Ingestion
{
    public class IngestionServiceTests
    {
        private readonly IIngestionStrategy _ingestionStrategy;
        private readonly IngestionService _ingestionService;

        public IngestionServiceTests()
        {
            _ingestionStrategy = Substitute.For<IIngestionStrategy>();
            _ingestionService = new IngestionService(_ingestionStrategy);
        }

        [Fact]
        public async Task GivenHL7v2IngestionDataType_WhenIngestIsCalled_ThenShouldReturnExpectedResult()
        {
            var ingestionRequest = new IngestionRequest("organisationCode", "sourceDomain", IngestionDataType.HL7v2, "ingestionMessage");
            var expectedResult = new Result();
            _ingestionStrategy.Ingest(ingestionRequest).Returns(expectedResult);

            var result = await _ingestionService.Ingest(ingestionRequest);

            result.ShouldBe(expectedResult);
            await _ingestionStrategy.Received(1).Ingest(ingestionRequest);
        }

        [Fact]
        public async Task GivenUnsupportedIngestionDataType_WhenIngestIsCalled_ThenShouldThrowInvalidOperationException()
        {
            var unsupportedDataTypeRequest =
                new IngestionRequest("organisationCode", "sourceDomain", (IngestionDataType)999, "ingestionMessage");

            var result = await _ingestionService.Ingest(unsupportedDataTypeRequest);
            result.IsFailure.ShouldBeTrue();
            result.Exception.ShouldBeOfType<InvalidOperationException>();
            result.Exception.Message.ShouldBe("Ingestion data type 999 is not supported");
        }
    }
}