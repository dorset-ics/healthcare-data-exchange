# Healthcare Data Exchange

[![OpenSSF Scorecard](https://api.scorecard.dev/projects/github.com/dorset-ics/healthcare-data-exchange/badge)](https://scorecard.dev/viewer/?uri=github.com/dorset-ics/healthcare-data-exchange)
[![OpenSSF Best Practices](https://www.bestpractices.dev/projects/8951/badge)](https://www.bestpractices.dev/projects/8951)
![CI](https://github.com/dorset-ics/healthcare-data-exchange/workflows/CI/badge.svg?branch=main)

This repository contains the source code for the Healthcare Data Exchange, a FHIR based integration and interoperability platform to support a regional healthcare network.

The solution integrates with UK national services such as the Personal Demographics Service, and MESH, and provides various options to ingest data from a typical NHS organisation.

Data is mapped to the UK Core R4 FHIR profiles, and made available through a standards compliant FHIR REST API.

For more information on UK Core, navigate to the [following page](https://simplifier.net/hl7fhirukcorer4).

## Getting started with Healthcare Data Exchange

Here is the [detailed guide](docs/setup-guide.md) to getting started with using Healthcare Data Exchange in your organisation.

## Project Documentation

### Pre-requisites

- [Docker](https://docs.docker.com/get-docker/)

### Running the documentation locally

The documentation is served using dockerized [MkDocs](https://www.mkdocs.org/).
To view the documentation, run the following command:

```bash
docker-compose -f ./docker/docker-compose-mkdocs.yml up
```

Then navigate to [http://localhost:8003](http://localhost:8003) in your browser.
