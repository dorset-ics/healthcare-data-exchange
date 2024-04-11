using Api.Extensions.Ingest;
using Api.Models.Ingest;
using Core.Ingestion.Enums;

namespace Unit.Tests.Api.Models.Ingest
{
    public class IngestionRequestParametersTests
    {
        [Fact]
        public void GivenNoParametersInTheModel_WhenToIngestionRequestIsCalled_ThenReturnsEmptyFhirParameters()
        {
            var model = new IngestionRequestParameters();

            var result = model.ToIngestionRequest("message");

            result.ShouldNotBeNull();
            result.OrganisationCode.ShouldBe(null);
            result.SourceDomain.ShouldBe(null);
            result.IngestionDataType.ShouldBe(IngestionDataType.HL7v2);
            result.Message.ShouldBe("message");
        }

        [Fact]
        public void GivenParametersInTheModel_WhenToIngestionRequestIsCalled_ThenReturnsIngestionRequestWithSameParameters()
        {
            var model = new IngestionRequestParameters { OrganisationCode = "org", SourceDomain = "source", IngestionDataType = IngestionDataType.HL7v2 };
            var result = model.ToIngestionRequest("message");

            result.ShouldNotBeNull();
            result.OrganisationCode.ShouldBe("org");
            result.SourceDomain.ShouldBe("source");
            result.IngestionDataType.ShouldBe(IngestionDataType.HL7v2);
            result.Message.ShouldBe("message");

        }

        [Fact]
        public void GivenNullMessage_WhenToIngestionRequest_ThenNullIsAssignedToRequest()
        {
            var model = new IngestionRequestParameters { OrganisationCode = "org", SourceDomain = "source", IngestionDataType = IngestionDataType.HL7v2 };

            var result = model.ToIngestionRequest(null!);

            result.Message.ShouldBeNull();
            result.OrganisationCode.ShouldBe("org");
            result.SourceDomain.ShouldBe("source");
            result.IngestionDataType.ShouldBe(IngestionDataType.HL7v2);
        }
    }
}