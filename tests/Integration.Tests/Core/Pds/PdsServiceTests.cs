using System.Globalization;
using System.Net.Mime;
using System.Text;
using Core;
using Core.Common.Extensions;
using Core.Pds.Abstractions;
using Core.Pds.Models;
using Core.Pds.Utilities;
using CsvHelper;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Infrastructure.DataHub.Clients.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NEL.MESH.Clients;
using NEL.MESH.Models.Configurations;
using NEL.MESH.Models.Foundations.Mesh;
using Task = System.Threading.Tasks.Task;

namespace Integration.Tests.Core.Pds;

public class PdsServiceTests : IDisposable
{
    private readonly ApiWebApplicationFactory _webApplicationFactory;
    private readonly IPdsService _sut;
    private readonly IDataHubFhirClientWrapper _dataHubFhirClientWrapper;

    public PdsServiceTests()
    {
        _webApplicationFactory = new ApiWebApplicationFactory();
        _dataHubFhirClientWrapper = _webApplicationFactory.Services.GetService<IDataHubFhirClientWrapper>()!;
        _sut = _webApplicationFactory.Services.GetService<IPdsService>()
               ?? throw new Exception("Failed to resolve IPdsService from the service provider");
    }

    public void Dispose()
    {
        _webApplicationFactory.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GivenSinglePageOfPatients_WhenSendingMeshMessages_ThenSingleMessageIsSent()
    {
        await CleanFhirStore();

        var patients = await ImportPatientsToFhirStore(3);

        await _sut.SendMeshMessages(new CancellationToken());

        var messages = await RetrieveMeshMessages();

        messages.Count().ShouldBe(1);

        AssertSentMessagesContainPatients(messages, patients);
    }

    [Fact]
    public async Task GivenMultiplePagesOfPatients_WhenSendingMeshMessages_ThenMultipleMessagesAreSent()
    {
        await CleanFhirStore();

        var patients = await ImportPatientsToFhirStore(Globals.FhirServerMaxPageSize + 1);

        await _sut.SendMeshMessages(new CancellationToken());

        var messages = await RetrieveMeshMessages();

        messages.Count().ShouldBe(2);

        AssertSentMessagesContainPatients(messages, patients);
    }

    [Fact]
    public async Task GivenSinglePageOfPatients_WhenRetrievingMeshMessages_ThenSingleMessageIsProcessed()
    {
        await CleanFhirStore();

        var patients = await ImportPatientsToFhirStore(3);

        await SendMeshMessages(patients);

        await _sut.RetrieveMeshMessages(new CancellationToken());

        await AssertRetrievalOfMessagesResultedInUpdatedPatientsAsync(patients);
    }

    [Fact]
    public async Task GivenMultiplePagesOfPatients_WhenRetrievingMeshMessages_ThenMultiplesMessageAreProcessed()
    {
        await CleanFhirStore();

        var patients = await ImportPatientsToFhirStore(Globals.FhirServerMaxPageSize + 1);

        await SendMeshMessages(patients);

        await _sut.RetrieveMeshMessages(new CancellationToken());

        await AssertRetrievalOfMessagesResultedInUpdatedPatientsAsync(patients);
    }

    private async Task AssertRetrievalOfMessagesResultedInUpdatedPatientsAsync(IEnumerable<Patient> patients)
    {
        foreach (var patient in patients)
        {
            var persistedPatient =
                (Patient)(await _dataHubFhirClientWrapper.SearchResourceByIdentifier<Patient>(patient.Id))
                .Entry
                .First()
                .Resource;

            persistedPatient.Name.Count.ShouldBe(1);
            persistedPatient.Name.First().Family = "UPDATED-FAMILY-NAME";
            persistedPatient.Name.First().Given.Any(given => given == "UPDATED-GIVEN-NAME").ShouldBeTrue();
            persistedPatient.Name.First().Given.Any(given => given == "UPDATED-OTHER-NAME").ShouldBeTrue();
            persistedPatient.Gender.ShouldBe(AdministrativeGender.Male);
            persistedPatient.Address.Count.ShouldBe(1);
            persistedPatient.Address.First().Line.Any(line => line == "UPDATED-ADDRESS1").ShouldBeTrue();
            persistedPatient.Address.First().Line.Any(line => line == "UPDATED-ADDRESS2").ShouldBeTrue();
            persistedPatient.Address.First().Line.Any(line => line == "UPDATED-ADDRESS3").ShouldBeTrue();
            persistedPatient.Address.First().Line.Any(line => line == "UPDATED-ADDRESS4").ShouldBeTrue();
            persistedPatient.Address.First().Line.Any(line => line == "UPDATED-ADDRESS5").ShouldBeTrue();
            persistedPatient.Address.First().PostalCode.ShouldBe("UPDATED-POSTCODE");
            persistedPatient.GeneralPractitioner.Count.ShouldBe(1);
            persistedPatient.GeneralPractitioner.First().Identifier.Value.ShouldBe("UPDATED-GP");
            persistedPatient.Telecom.First(telecom =>
                telecom.System == ContactPoint.ContactPointSystem.Phone
                && telecom.Use == ContactPoint.ContactPointUse.Mobile).Value.ShouldBe("UPDATED-MOBILETEL");
            persistedPatient.Telecom.First(telecom =>
                telecom.System == ContactPoint.ContactPointSystem.Phone
                && telecom.Use == ContactPoint.ContactPointUse.Home).Value.ShouldBe("UPDATED-HOMETEL");
            persistedPatient.Telecom.First(telecom => telecom.System == ContactPoint.ContactPointSystem.Email)
                .Value.ShouldBe("UPDATED-EMAIL");
        }
    }

    private void AssertSentMessagesContainPatients(IEnumerable<Message> messages, IEnumerable<Patient> patients)
    {
        foreach (var message in messages)
        {
            var csvLines = Encoding.UTF8.GetString(message.FileContent)
                .SplitLines()
                .Skip(1)
                .Prepend(PdsMeshUtilities.GetPdsMeshRecordRequestHeaderLine());

            var csv = string.Join(Environment.NewLine, csvLines);

            var sourceReader = new StringReader(csv);
            using var csvReader = new CsvReader(sourceReader, CultureInfo.CurrentCulture);
            var pdsMeshRecords = csvReader.GetRecords<PdsMeshRecordRequest>();

            foreach (var record in pdsMeshRecords)
            {
                var patient = patients.SingleOrDefault(p => p.Id == record.UniqueReference);

                patient.ShouldNotBeNull();
                record.UniqueReference.ShouldBe(patient.Id);
                record.NhsNumber.ShouldBe(patient.GetNhsNumber());
            }
        }
    }

    private async Task SendMeshMessages(IEnumerable<Patient> patients)
    {
        var meshClient = GetMeshClient();

        for (var pageIndex = 0; pageIndex <= (patients.Count() / Globals.FhirServerMaxPageSize); pageIndex++)
        {
            var page = patients.Skip(Globals.FhirServerMaxPageSize * pageIndex)
                .Take(Globals.FhirServerMaxPageSize);

            var csv = string.Join(Environment.NewLine, page.Select(p =>
            {
                return string.Join(",",
                    new string[]
                    {
                        p.Id, p.GetNhsNumber()!, "UPDATED-FAMILY-NAME", "UPDATED-GIVEN-NAME", "UPDATED-OTHER-NAME",
                        "1", "20010101", string.Empty, "UPDATED-ADDRESS1", "UPDATED-ADDRESS2", "UPDATED-ADDRESS3",
                        "UPDATED-ADDRESS4", "UPDATED-ADDRESS5", "UPDATED-POSTCODE", "UPDATED-GP", string.Empty,
                        string.Empty, string.Empty, string.Empty, string.Empty, "UPDATED-HOMETEL",
                        "UPDATED-MOBILETEL", "UPDATED-EMAIL", string.Empty, string.Empty, string.Empty,
                        p.GetNhsNumber()!, string.Empty, string.Empty,
                    });
            }).Prepend(PdsMeshUtilities.GetPdsMeshRecordResponseHeaderLine()));

            await meshClient.Mailbox.SendMessageAsync(
                "X26ABC1",
                "SPINE_PDS_MESH_V1",
                csv,
                mexFileName: $"RESP_MPTREQ_{DateTime.Now:yyyyMMddHHmmss}_{DateTime.Now:yyyyMMddHHmmss}.csv",
                contentType: MediaTypeNames.Text.Csv);
        }
    }

    private async Task<IEnumerable<Message>> RetrieveMeshMessages()
    {
        var messages = new List<Message>();

        var meshClient = GetMeshClient();

        var messageIds = await meshClient.Mailbox.RetrieveMessagesAsync();

        foreach (var messageId in messageIds)
        {
            var message = await meshClient.Mailbox.RetrieveMessageAsync(messageId);

            messages.Add(message);

            await meshClient.Mailbox.AcknowledgeMessageAsync(messageId);
        }

        return messages;
    }

    private static MeshClient GetMeshClient()
    {
        var config = new MeshConfiguration
        {
            MailboxId = "X26ABC1",
            Password = "password",
            Key = "TestKey",
            Url = "http://localhost:8700",
            MaxChunkSizeInMegabytes = 100
        };

        return new MeshClient(config);
    }

    private async Task CleanFhirStore()
    {
        var searchResult =
            await _dataHubFhirClientWrapper.SearchResourceByParams<Patient>(
                new SearchParams().LimitTo(Globals.FhirServerMaxPageSize));

        while (searchResult != null)
        {
            foreach (var patient in searchResult.Entry)
                await _dataHubFhirClientWrapper.DeleteAsync($"{patient.Resource.TypeName}/{patient.Resource.Id}");

            searchResult = await _dataHubFhirClientWrapper.ContinueAsync(searchResult!);

            if (searchResult == null)
                break;
        }
    }

    private async Task<IEnumerable<Patient>> ImportPatientsToFhirStore(int numberOfPatients)
    {
        var patients = new List<Patient>();

        for (var i = 0; i < numberOfPatients; i++)
        {
            var nhsNumber = Guid.NewGuid().ToString();

            var patient = new Patient()
            {
                Id = nhsNumber,
                Meta =
                    new Meta()
                    {
                        Profile = new List<string>()
                        {
                            "https://fhir.hl7.org.uk/StructureDefinition/UKCore-Patient"
                        }
                    },
                Identifier =
                    new List<Identifier>() { new Identifier("https://fhir.nhs.uk/Id/nhs-number", nhsNumber) },
                Name =
                    new List<HumanName>()
                    {
                        new HumanName()
                        {
                            Family = "TO-BE-UPDATED", Given = new List<string>() { "TO-BE-UPDATED" }
                        }
                    },
                Gender = AdministrativeGender.Unknown,
                BirthDate = "1920-01-01",
                Address =
                    new List<Address>()
                    {
                        new Address()
                        {
                            Line = new List<string>()
                            {
                                "TO-BE-UPDATED1",
                                "TO-BE-UPDATED2",
                                "TO-BE-UPDATED3",
                                "TO-BE-UPDATED4",
                                "TO-BE-UPDATED5"
                            },
                            PostalCode = "TO-BE-UPDATED"
                        }
                    },
                Telecom = new List<ContactPoint>()
                {
                    new ContactPoint()
                    {
                        System = ContactPoint.ContactPointSystem.Phone,
                        Value = "TO-BE-UPDATED",
                        Use = ContactPoint.ContactPointUse.Home
                    },
                    new ContactPoint()
                    {
                        System = ContactPoint.ContactPointSystem.Phone,
                        Value = "TO-BE-UPDATED",
                        Use = ContactPoint.ContactPointUse.Mobile
                    },
                    new ContactPoint()
                    {
                        System = ContactPoint.ContactPointSystem.Email, Value = "TO-BE-UPDATED"
                    }
                },
                GeneralPractitioner = new List<ResourceReference>()
                {
                    new ResourceReference()
                    {
                        Identifier = new Identifier()
                        {
                            System = "https://fhir.nhs.uk/Id/ods-organization-code",
                            Value = "TO-BE-UPDATED"
                        }
                    }
                }
            };

            await _dataHubFhirClientWrapper.UpdateAsync<Patient>(patient);

            patients.Add(patient);
        }

        return patients;
    }
}