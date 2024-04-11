using Api.Extensions.Patient;
using Api.Models.Patient;

namespace Unit.Tests.Api.Extensions.Patient;

public class SearchModelExtensionsTests
{
    [Fact]
    public void WhenConvertingToPdsSearchParameters_ShouldMapCorrectly()
    {
        var searchModel = new SearchModel
        {
            FamilyName = "Bloggs",
            GivenName = "Joe",
            Gender = "Female",
            Postcode = "DT1 2JY",
            DateOfBirth = "1983-03-04",
            DateOfDeath = "2001-01-01",
            RegisteredGpPractice = "GP1",
            EmailAddress = "joe.bloggs@microsoft.com",
            PhoneNumber = "01305 251212",
            Identifier = "id1",
            IsFuzzyMatch = "Y",
            IsExactMatch = "Y",
            IsHistorySearch = "Y"
        };

        var pdsSearchParameters = searchModel.ToPdsSearchParameters();

        pdsSearchParameters.FamilyName.ShouldBe(searchModel.FamilyName);
        pdsSearchParameters.GivenName.ShouldBe(searchModel.GivenName);
        pdsSearchParameters.Gender.ShouldBe(searchModel.Gender);
        pdsSearchParameters.Postcode.ShouldBe(searchModel.Postcode);
        pdsSearchParameters.DateOfBirth.ShouldBe(searchModel.DateOfBirth);
        pdsSearchParameters.DateOfDeath.ShouldBe(searchModel.DateOfDeath);
        pdsSearchParameters.RegisteredGpPractice.ShouldBe(searchModel.RegisteredGpPractice);
        pdsSearchParameters.EmailAddress.ShouldBe(searchModel.EmailAddress);
        pdsSearchParameters.PhoneNumber.ShouldBe(searchModel.PhoneNumber);
        pdsSearchParameters.Identifier.ShouldBe(searchModel.Identifier);
        pdsSearchParameters.IsFuzzyMatch.ShouldBe(searchModel.IsFuzzyMatch);
        pdsSearchParameters.IsExactMatch.ShouldBe(searchModel.IsExactMatch);
        pdsSearchParameters.IsHistorySearch.ShouldBe(searchModel.IsHistorySearch);
    }
}