using System.Globalization;
using System.Net.Mime;
using Core;
using Core.Common.Extensions;
using Core.Pds.Models;
using Core.Pds.Utilities;
using CsvHelper;

namespace E2E.Tests.PdsMesh;

public class PdsMeshTests(ITestOutputHelper outputHelper) : BaseApiTest(outputHelper)
{
    // NOTE:
    // These tests are relying on the fact that the RetrieveMeshMessages Job is triggered frequently enough to pick up the message
    // If the job will not run within a minute, the tests will fail.
    // the tests should run with an override of the RetrieveMeshMessages job schedule Pds__Mesh__RetrieveSchedule="*/10 0 0 * * ?"

    // Another important concept in this test, is that we send a message which matches the PDS response. The PDS mesh background
    // task will then pick that up as if it came from PDS mesh. This approach is required as the PDS sandbox environment does not
    // work like a real mesh environment - it will not receive a response for a PDS mesh request.

    [Fact]
    public async Task GivenMessageInInboxWithSinglePatient_WhenPatientDoesNotExist_ThenPatientIsInserted()
    {
        var nhsNumber = "9990554412";

        EnsurePatientDoesNotExist(nhsNumber);

        var messageContent = await File.ReadAllTextAsync("PdsMesh/Samples/MeshResponseSinglePatient.csv");

        var dateMeshMessageSent = DateTime.UtcNow;

        await SendPdsMeshMessage(messageContent);

        InvokePdsRetrieveMessages();

        AssertPdsMeshResponseMessageHandledCorrectly(messageContent, dateMeshMessageSent);
    }

    [Fact]
    public async Task GivenMessageInInboxWithMultiplePatients_WhenPatientsDoNotExist_ThenPatientsAreInserted()
    {
        var nhsNumbers = new string[] { "9990554413", "9990554414", "9990554415" };

        nhsNumbers.AsParallel().ForAll(EnsurePatientDoesNotExist);

        var messageContent = await File.ReadAllTextAsync("PdsMesh/Samples/MeshResponseMultiplePatients.csv");

        var dateMeshMessageSent = DateTime.UtcNow;

        await SendPdsMeshMessage(messageContent);

        InvokePdsRetrieveMessages();

        AssertPdsMeshResponseMessageHandledCorrectly(messageContent, dateMeshMessageSent);
    }

    [Fact]
    public async Task GivenMessageInInboxWithSinglePatient_WhenPatientAlreadyExists_ThenPatientIsUpdated()
    {
        var nhsNumber = "9990554412";

        EnsurePatientDoesNotExist(nhsNumber);

        CreatePlaceholderPatient(nhsNumber);

        var messageContent = await File.ReadAllTextAsync("PdsMesh/Samples/MeshResponseSinglePatient.csv");

        var dateMeshMessageSent = DateTime.UtcNow;

        await SendPdsMeshMessage(messageContent);

        InvokePdsRetrieveMessages();

        AssertPdsMeshResponseMessageHandledCorrectly(messageContent, dateMeshMessageSent);
    }

    [Fact]
    public async Task GivenMessageInInboxWithMultiplePatients_WhenPatientsAlreadyExist_ThenPatientsAreUpdated()
    {
        var nhsNumbers = new string[] { "9990554413", "9990554414", "9990554415" };

        nhsNumbers.AsParallel().ForAll(EnsurePatientDoesNotExist);

        nhsNumbers.AsParallel().ForAll(CreatePlaceholderPatient);

        var messageContent = await File.ReadAllTextAsync("PdsMesh/Samples/MeshResponseMultiplePatients.csv");

        var dateMeshMessageSent = DateTime.UtcNow;

        await SendPdsMeshMessage(messageContent);

        InvokePdsRetrieveMessages();

        AssertPdsMeshResponseMessageHandledCorrectly(messageContent, dateMeshMessageSent);
    }

    private void InvokePdsRetrieveMessages()
    {
        var pdsRequest = Post("/internal/run/pds", null, null);
        var runPdsRequest = ApiClient.Execute(pdsRequest);
        runPdsRequest.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private async Task SendPdsMeshMessage(string messageContent)
    {
        var meshClient = GetMeshClient(MeshClientName.Pds);
        var meshSettings = MeshSettings[MeshClientName.Pds];
        var resp = await meshClient.Mailbox.SendMessageAsync(
            meshSettings.MailboxId,
            meshSettings.WorkflowId,
            messageContent,
            mexFileName: $"RESP_MPTREQ_{DateTime.Now:yyyyMMddHHmmss}_{DateTime.Now:yyyyMMddHHmmss}.csv",
            contentType: MediaTypeNames.Text.Csv);
        resp.MessageId.ShouldNotBeEmpty();
        return;
    }

    private void EnsurePatientDoesNotExist(string nhsNumber)
    {
        FhirClient.Delete(new RestRequest($"/Patient/{nhsNumber}"));
    }

    private void AssertPdsMeshResponseMessageHandledCorrectly(string messageContent, DateTime dateMeshMessageSent)
    {
        var csvLines = messageContent
            .SplitLines()
            .Skip(1)
            .Prepend(PdsMeshUtilities.GetPdsMeshRecordResponseHeaderLine());

        var csv = string.Join(Environment.NewLine, csvLines);

        var sourceReader = new StringReader(csv);
        using var csvReader = new CsvReader(sourceReader, CultureInfo.CurrentCulture);
        var pdsMeshRecords = csvReader.GetRecords<PdsMeshRecordResponse>();

        foreach (var pdsMeshRecord in pdsMeshRecords)
            AssertPdsMeshRecordImported(pdsMeshRecord, dateMeshMessageSent);
    }

    private void AssertPdsMeshRecordImported(PdsMeshRecordResponse pdsMeshRecord, DateTime dateMeshMessageSent)
    {
        var patientResponse =
            FhirClient.Execute(Get(
                $"/Patient?identifier={pdsMeshRecord.NhsNumber}&_lastUpdated=ge{dateMeshMessageSent.ToString("yyyy-MM-ddTHH:mm:ss.fff")}"));

        if (!patientResponse.Content!.Contains("entry"))
            patientResponse.IsSuccessStatusCode = false;

        patientResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        patientResponse.Content.ShouldNotBeEmpty();

        var patientJson = JToken.Parse(patientResponse.Content!)?.SelectToken("entry[*].resource")!;

        patientJson.SelectTokens("identifier[*].system")
            .Any(s => s.Value<string>() == "https://fhir.nhs.uk/Id/nhs-number").ShouldBeTrue();
        patientJson.SelectTokens("identifier[*].value").Any(s => s.Value<string>() == pdsMeshRecord.NhsNumber)
            .ShouldBeTrue();
        patientJson.SelectTokens("name[*].family").Any(s => s.Value<string>() == pdsMeshRecord.FamilyName)
            .ShouldBeTrue();
        patientJson.SelectTokens("name[*].given[*]").Any(s => s.Value<string>() == pdsMeshRecord.GivenName)
            .ShouldBeTrue();
        patientJson.SelectTokens("name[*].given[*]").Any(s => s.Value<string>() == pdsMeshRecord.OtherGivenName)
            .ShouldBeTrue();
        patientJson.SelectTokens("gender").First().Value<string>().ShouldBe(MapGenderCodeToValue(pdsMeshRecord.Gender));
        patientJson.SelectTokens("birthDate").First().Value<string>().ShouldBe(
            $"{pdsMeshRecord.DateOfBirth!.Substring(0, 4)}-{pdsMeshRecord.DateOfBirth.Substring(4, 2)}-{pdsMeshRecord.DateOfBirth.Substring(6, 2)}");
        patientJson.SelectTokens("deceasedDateTime").First().Value<string>().ShouldBe(
            $"{pdsMeshRecord.DateOfDeath!.Substring(0, 4)}-{pdsMeshRecord.DateOfDeath.Substring(4, 2)}-{pdsMeshRecord.DateOfDeath.Substring(6, 2)}");
        patientJson.SelectTokens("address[*].line[*]").Any(s => s.Value<string>() == pdsMeshRecord.AddressLine1)
            .ShouldBeTrue();
        patientJson.SelectTokens("address[*].line[*]").Any(s => s.Value<string>() == pdsMeshRecord.AddressLine2)
            .ShouldBeTrue();
        patientJson.SelectTokens("address[*].line[*]").Any(s => s.Value<string>() == pdsMeshRecord.AddressLine3)
            .ShouldBeTrue();
        patientJson.SelectTokens("address[*].line[*]").Any(s => s.Value<string>() == pdsMeshRecord.AddressLine4)
            .ShouldBeTrue();
        patientJson.SelectTokens("address[*].line[*]").Any(s => s.Value<string>() == pdsMeshRecord.AddressLine5)
            .ShouldBeTrue();
        patientJson.SelectTokens("address[*].postalCode").Any(s => s.Value<string>() == pdsMeshRecord.Postcode)
            .ShouldBeTrue();
        patientJson.SelectTokens("telecom[*].system").Count(s => s.Value<string>() == "phone").ShouldBe(2);
        patientJson.SelectTokens("telecom[*].use").Any(s => s.Value<string>() == "home").ShouldBeTrue();
        patientJson.SelectTokens("telecom[*].value").Any(s => s.Value<string>() == pdsMeshRecord.TelephoneNumber)
            .ShouldBeTrue();
        patientJson.SelectTokens("telecom[*].use").Any(s => s.Value<string>() == "mobile").ShouldBeTrue();
        patientJson.SelectTokens("telecom[*].value").Any(s => s.Value<string>() == pdsMeshRecord.MobileNumber)
            .ShouldBeTrue();
        patientJson.SelectTokens("telecom[*].system").Count(s => s.Value<string>() == "email").ShouldBe(1);
        patientJson.SelectTokens("telecom[*].value").Any(s => s.Value<string>() == pdsMeshRecord.EmailAddress)
            .ShouldBeTrue();
        patientJson.SelectTokens("generalPractitioner[*].identifier.system")
            .Any(s => s.Value<string>() == "https://fhir.nhs.uk/Id/ods-organization-code").ShouldBeTrue();
        patientJson.SelectTokens("generalPractitioner[*].identifier.value")
            .Any(s => s.Value<string>() == pdsMeshRecord.GpPracticeCode).ShouldBeTrue();
    }

    private static string? MapGenderCodeToValue(string? gender)
    {
        switch (gender)
        {
            case "0": return "unknown";
            case "1": return "male";
            case "2": return "female";
            default: return "unknown";
        }
    }

    private void CreatePlaceholderPatient(string nhsNumber)
    {
        var patientJson = $@"
        {{
            ""resourceType"": ""Patient"",
            ""id"": ""{nhsNumber}"",
            ""meta"": {{
                ""versionId"": ""2"",
                ""source"": ""Organization/{Globals.X26OrganizationResourceId}"",
                ""security"": [
                  {{
                    ""system"": ""http://terminology.hl7.org/CodeSystem/v3-Confidentiality"",
                    ""code"": ""U"",
                    ""display"": ""unrestricted""
                  }}
                ],
                ""profile"": [
                    ""https://fhir.hl7.org.uk/StructureDefinition/UKCore-Patient""
                ]
            }},
            ""identifier"": [
                {{
                  ""system"": ""https://fhir.nhs.uk/Id/nhs-number"",
                  ""value"": ""{nhsNumber}""
                }}
            ],
            ""name"": [
                {{
                  ""family"": ""TO-BE-UPDATED"",
                  ""given"": [""TO-BE-UPDATED""]
                }}
            ],
            ""gender"": ""unknown"",
            ""birthDate"": ""1920-01-01"",
            ""deceasedDateTime"": ""1920-01-01"",
            ""address"": [
                {{
                  ""line"": [
                    ""TO-BE-UPDATED"",
                    ""TO-BE-UPDATED"",
                    ""TO-BE-UPDATED"",
                    ""TO-BE-UPDATED"",
                    ""TO-BE-UPDATED""
                  ],
                  ""postalCode"": ""TO-BE-UPDATED""
                }}
            ],
            ""telecom"": [
                {{
                 ""system"": ""phone"",
                 ""value"": ""TO-BE-UPDATED"",
                 ""use"": ""home""
                }},
                {{
                 ""system"": ""phone"",
                 ""value"": ""TO-BE-UPDATED"",
                 ""use"": ""mobile""
                }},
                {{
                  ""system"": ""email"",
                  ""value"": ""TO-BE-UPDATED""
                }}
            ],
            ""generalPractitioner"": [
                  {{
                    ""type"": ""Organization"",
                    ""identifier"": {{
                      ""system"": ""https://fhir.nhs.uk/Id/ods-organization-code"",
                      ""value"":  ""TO-BE-UPDATED""
                      }}
                    }}
            ]
        }}";

        var request = new RestRequest()
        {
            Resource = $"/Patient/{nhsNumber}",
            Method = Method.Put,
            RequestFormat = DataFormat.Json
        };

        request.AddBody(patientJson);

        FhirClient.Put(request);
    }
}