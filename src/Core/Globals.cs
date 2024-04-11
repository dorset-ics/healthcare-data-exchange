namespace Core;

public static class Globals
{
    public const string NhsNumberSystem = "https://fhir.nhs.uk/Id/nhs-number";

    // Azure FHIR Bundle size is limited to 500:
    // https://learn.microsoft.com/en-us/azure/healthcare-apis/fhir/fhir-features-supported#service-limits
    public const int FhirServerMaxPageSize = 500;

    public const string PdsMeshMessageFileNamePrefix = "MPTREQ";
    public const string NdopMeshMessageFileNamePrefix = "NDOPREQ";
    public const string MeshFileNameHeader = "mex-filename";

    public const string HL7v2Version = "2.4";
    public const string DataPlatformName = "DEX";
    public const string SendingFacility = "QVV";
    public const string X26OrganizationResourceId = "cec48f09-f30e-cb9f-adc1-50e79d71796d";

    public static class PdsSearchQueryStringNames
    {
        public const string FamilyName = "family";
        public const string GivenName = "given";
        public const string Gender = "gender";
        public const string Postcode = "address-postalcode";
        public const string DateOfBirth = "birthdate";
        public const string DateOfDeath = "death-date";
        public const string RegisteredGpPractice = "general-practitioner";
        public const string EmailAddress = "email";
        public const string PhoneNumber = "phone";
        public const string Identifier = "identifier";
        public const string IsFuzzyMatch = "_fuzzy-match";
        public const string IsExactMatch = "_exact-match";
        public const string IsHistorySearch = "_history";
    }
}