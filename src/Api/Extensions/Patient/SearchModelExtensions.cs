using Api.Models.Patient;
using Core.Pds.Models;

namespace Api.Extensions.Patient
{
    public static class SearchModelExtensions
    {
        public static PdsSearchParameters ToPdsSearchParameters(this SearchModel model)
        {
            return new PdsSearchParameters()
            {
                FamilyName = model.FamilyName,
                GivenName = model.GivenName,
                Gender = model.Gender,
                Postcode = model.Postcode,
                DateOfBirth = model.DateOfBirth,
                DateOfDeath = model.DateOfDeath,
                RegisteredGpPractice = model.RegisteredGpPractice,
                EmailAddress = model.EmailAddress,
                PhoneNumber = model.PhoneNumber,
                Identifier = model.Identifier,
                IsFuzzyMatch = model.IsFuzzyMatch,
                IsExactMatch = model.IsExactMatch,
                IsHistorySearch = model.IsHistorySearch
            };
        }
    }
}
