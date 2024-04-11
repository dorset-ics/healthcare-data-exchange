# PDS MESH Mapping

This is a table specifying PDS mesh response attributes and the FHIR resource attributes they map to.

| Source          | Destination                    | Comment                                                                                                                           |
|-----------------|--------------------------------|-----------------------------------------------------------------------------------------------------------------------------------|
| UniqueReference | id, Bundle.fullUrl             |                                                                                                                                   |
| NhsNumber       | identifier.value               |                                                                                                                                   |
| FamilyName      | name.family                    |                                                                                                                                   |
| GivenName       | name.given                     |                                                                                                                                   |
| OtherGivenName  | name.given                     |                                                                                                                                   |
| Gender          | gender                         | Gender (sex) of the person, values: 0 = Not Known, 1 = Male, 2 = Female, 9 = Not Specified                                        |
| DateOfBirth     | birthDate                      | In one of the following formats: full date and time (YYYYMMDDHHMM), full date (YYYYMMDD), year & month (YYYYMM), year only (YYYY) |
| AddressLine1    | address.line                   |                                                                                                                                   |
| AddressLine2    | address.line                   |                                                                                                                                   |
| AddressLine3    | address.line                   |                                                                                                                                   |
| AddressLine4    | address.line                   |                                                                                                                                   |
| AddressLine5    | address.line                   |                                                                                                                                   |
| Postcode        | address.postcode               |                                                                                                                                   |
| TelephoneNumber | telecom                        |                                                                                                                                   |
| MobileNumber    | telecom                        |                                                                                                                                   |
| EmailAddress    | telecom                        |                                                                                                                                   |
| GpPracticeCode  | generalPractitioner.identifier |                                                                                                                                   |
