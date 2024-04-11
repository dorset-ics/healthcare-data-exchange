# SNOMED Code System

The data ingestion pipeline implements an `IFhirResourceEnhancer` to map SNOMED codes to their corresponding concept descriptions. This is invoked after data conversion and before the `Bundle` is persisted.

The resource enhancer searches the FHIR bundle for any `CodeableConcept` elements with the SNOMED system URL. It then queries the SNOMED code system for the respective concept description.

This implementation does not currently support terminology queries against a FHIR terminology server. At present, the query is made against a JSON file containing the dictionary of descriptions. This file is located in "src/Infrastructure/Terminology/SnomedCodes.json".

## Using the SNOMED Code System

### Populating the SNOMED Code System

You must first populate the JSON file (`SnomedCodes.json`) with the SNOMED codes you wish to map.

The file is loaded into memory and used to map SNOMED codes to their descriptions. The file should be structured as follows:

```json
{
  "123456789": "Concept description",
  "987654321": "Another concept description"
}
```

### Output a `CodeableConcept` with SNOMED System URL

To make use of this enrichment process, simply include a `CodeableConcept` element in your FHIR resource with the SNOMED system URL. The resource enhancer will automatically lookup the code in the JSON file, and populate its description.

For example:

```json
{
  "resourceType": "Procedure",
  "code": {
    "coding": [
      {
        "system": "http://snomed.info/sct",
        "code": "123456789"
      }
    ]
  }
}
```

Will be transformed to:

```json
{
  "resourceType": "Procedure",
  "code": {
    "coding": [
      {
        "system": "http://snomed.info/sct",
        "code": "123456789",
        "display": "Concept description"
      }
    ]
  }
}
```
