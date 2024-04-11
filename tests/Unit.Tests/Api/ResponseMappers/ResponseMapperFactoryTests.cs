using Api.ResponseMappers;
using Core.Ingestion.Enums;
using Core.Ingestion.Models;

namespace Unit.Tests.Api.ResponseMappers;

public class ResponseMapperFactoryTests
{
    [Fact]
    public void Create_ShouldReturnHL7v2ResponseMapper_WhenIngestionDataTypeIsHL7v2()
    {
        var factory = new ResponseMapperFactory();
        var parameters = new IngestionRequest(
            string.Empty,
            string.Empty,
            IngestionDataType.HL7v2,
            string.Empty
        );

        var result = factory.Create(parameters);
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeOfType<HL7v2ResponseMapper>();
    }

    [Fact]
    public void Create_ShouldThrowNotSupportedException_WhenIngestionDataTypeIsNotSupported()
    {
        var factory = new ResponseMapperFactory();
        var parameters = new IngestionRequest(
            string.Empty,
            string.Empty,
            (IngestionDataType)999,
            string.Empty
        );
        var result = factory.Create(parameters);
        result.IsFailure.ShouldBeTrue();
        result.Exception.ShouldNotBeNull();
        result.Exception.Message.ShouldBe("IngestionDataType 999 is not supported");
    }
}