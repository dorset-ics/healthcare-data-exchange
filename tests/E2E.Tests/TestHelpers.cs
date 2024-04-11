namespace E2E.Tests;

public static class TestHelpers
{
    public static List<KeyValuePair<string, string>> GetIngestionHeaders(string organisation, string dataType, string sourceDomain)
    {
        return new List<KeyValuePair<string, string>>
        {
            new("organisation-code", organisation), new("data-type", dataType), new("source-domain", sourceDomain), new("Content-Type", "text/plain"), new("Accept", "text/plain")
        };
    }
}