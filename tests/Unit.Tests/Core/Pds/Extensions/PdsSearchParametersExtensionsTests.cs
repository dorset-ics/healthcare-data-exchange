using Core;
using Core.Pds.Extensions;
using Core.Pds.Models;

namespace Unit.Tests.Core.Pds.Extensions;

public class PdsSearchParametersExtensionsTests
{
    [Fact]
    public void ToFhirSearchParameters_WithNoParametersInTheModel_ThenReturnsEmptyFhirParameters()
    {
        var model = new PdsSearchParameters();

        var result = model.ToFhirSearchParameters();

        result.ShouldNotBeNull();
        result.Parameters.ShouldBeEmpty();
    }

    [Fact]
    public void ToFhirSearchParameters_WithSingleParameterInTheModel_ThenReturnsFhirParametersWithSingleParameter()
    {
        var model = new PdsSearchParameters() { FamilyName = "Bloggs" };

        var result = model.ToFhirSearchParameters();

        result.Parameters.Count.ShouldBe(1);
        result.Parameters.Single().Item2.ShouldBe("Bloggs");
    }

    [Fact]
    public void ToFhirSearchParameters_WithMultipleParametersInTheModel_ThenReturnsFhirParametersWithMultipleParameters()
    {
        var model = new PdsSearchParameters() { FamilyName = "Bloggs", GivenName = "Joe", DateOfBirth = "2001-01-01" };

        var result = model.ToFhirSearchParameters();

        result.Parameters.Count.ShouldBe(3);
        result.Parameters.SingleOrDefault(p => p.Item2 == "Bloggs").ShouldNotBeNull();
        result.Parameters.SingleOrDefault(p => p.Item2 == "Joe").ShouldNotBeNull();
        result.Parameters.SingleOrDefault(p => p.Item2 == "2001-01-01").ShouldNotBeNull();
    }

    [Fact]
    public void ToFhirSearchParameters_WithEmptyAndWhitespaceParameters_ThenDoesNotAddToFhirParameters()
    {
        var model = new PdsSearchParameters() { FamilyName = "Bloggs", GivenName = string.Empty, DateOfBirth = "     " };

        var result = model.ToFhirSearchParameters();

        result.Parameters.Count.ShouldBe(1);
        result.Parameters.SingleOrDefault(p => p.Item2 == "Bloggs").ShouldNotBeNull();
    }

    [Fact]
    public void ToFhirSearchParameters_WithParametersInTheModel_ThenReturnsFhirParametersKeyedToQuerystringName()
    {
        var model = new PdsSearchParameters() { FamilyName = "Bloggs", GivenName = "Joe", DateOfBirth = "2001-01-01" };

        var result = model.ToFhirSearchParameters();

        result.Parameters.Count.ShouldBe(3);
        result.Parameters.SingleOrDefault(p => p.Item1 == Globals.PdsSearchQueryStringNames.FamilyName).ShouldNotBeNull();
        result.Parameters.SingleOrDefault(p => p.Item1 == Globals.PdsSearchQueryStringNames.GivenName).ShouldNotBeNull();
        result.Parameters.SingleOrDefault(p => p.Item1 == Globals.PdsSearchQueryStringNames.DateOfBirth).ShouldNotBeNull();
    }
}