# National Data Opt-Out

The data exchange will integrate with the NDOP APIs to import Consent information and keep the information up to date.

## APIs

See the following links for details about the APIs:

* NDOP FHIR API - <https://digital.nhs.uk/developer/api-catalogue/national-data-opt-out-fhir>
* NDOP MESH API - <https://digital.nhs.uk/developer/api-catalogue/national-data-opt-out-service-mesh>

> NOTE: Currently there are no plans to integrate with the NDOP FHIR API.

## NDOP MESH

The NDOP MESH API is used for refreshing the Consent resources persisted in the system as a batch process, executed on a regular schedule.

The interactions with the API involve uploading a request to the outbox of the MESH mailbox linked to the NDOP MESH workflow, and
polling the inbox to check for a response. Once a response is available, it is downloaded and processed by the application.

### Request

The request consists of a data file which contains the patients that we require updated consent information for and a corresponding control file.

The data file must contain the fields
specified by the NDOP MESH workflow, and the control file must contain metadata specific to the request.

#### Data File

> The data file consists of two fields - the NHS Number and an empty field. An example of a data line is: `1234567890,\r\n`

Details of the format of the data file can be seen here:

<https://digital.nhs.uk/services/national-data-opt-out/compliance-with-the-national-data-opt-out/check-for-national-data-opt-outs-service#create-the-dat-file-of-patient-data-to-send>

#### Control File

Details of the format of the control file can be seen here:

<https://digital.nhs.uk/services/message-exchange-for-social-care-and-health-mesh/mesh-guidance-hub/client-user-guide>

The specific values which must go into control file for NDOP can be seen here:

<https://digital.nhs.uk/services/national-data-opt-out/compliance-with-the-national-data-opt-out/check-for-national-data-opt-outs-service#send-and-receive-your-file-over-mesh-client-api>

### Response

The response consists of a single CSV file containing the patients which have **not** opted out. The response must be processed alongside the request, in order to know who has opted out and update the FHIR store accordingly.
