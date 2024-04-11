# Personal Demographics Service

The data exchange will integrate with the PDS APIs to import patients and keep their information up to date.

## Design

This diagram shows the intended architecture for integration with the PDS APIs, including the persistence of PDS data and how the persisted data will be refreshed.

![PDS Integration Design](../assets/pds-integration-design.jpeg "PDS Integration Design")

## APIs

See the following links for details about the APIs:

* PDS FHIR API - <https://digital.nhs.uk/developer/api-catalogue/personal-demographics-service-fhir>
* PDS MESH API - <https://digital.nhs.uk/developer/api-catalogue/personal-demographic-service-mesh>
* PDS NEMS API - <https://digital.nhs.uk/developer/api-catalogue/personal-demographics-service-notifications-fhir>

## Test Data

PDS test data is generated in the NHSD Test Data Self Service Portal. The portal will generate a number of test patients and import them
into the specified PDS test environments. The patients can then be downloaded from the portal in Excel format.

The test data generated for this project can be found in the test data folder.

## Test Data Self Service Portal

The portal can be accessed at the following URL:

<http://testdatacloud.co.uk:8080/ords/apex-prod/f?p=159:LOGIN>

The username/password are stored in the development key vault.

## PDS MESH

The PDS MESH API is used for refreshing the Patient resources persisted in the system as a batch process, executed on a regular schedule.

The interactions with the API involve uploading a request to the outbox of the MESH mailbox linked to the PDS MESH workflow, and
polling the inbox to check for a response. Once a response is available, it is downloaded and processed by the application.

### Request

The request consists of a single CSV which contains the patients that we require updated details for. It must contain the fields specified
by the PDS MESH workflow. Details of the format of the request file can be seen here:

<https://digital.nhs.uk/developer/api-catalogue/personal-demographic-service-mesh/pds-mesh-data-dictionary#request-file-format>

### Response

The response consists of a single CSV file containing the latest information for each patient included in the request file. Details of
the format of the response file can be seen here:

<https://digital.nhs.uk/developer/api-catalogue/personal-demographic-service-mesh/pds-mesh-data-dictionary#response-file>
