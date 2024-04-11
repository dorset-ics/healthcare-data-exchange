# Testing and Validation

This document provides a comprehensive guide on how to run and manage the tests for the liquid templates in this
project.

## Overview

The test suite is designed to validate the functionality of the liquid templates. It does this by comparing the output
of a template for a given input, with an expected output. If an expected output file does not exist, the test will
create it. If it does exist, the test will compare the converted version with the expected output. When a template is
modified, the author should commit the relevant outputs, which will then be presented in the PR for review.

## Directory Structure

The directory structure for the tests is designed to mirror the structure of the `templates` directory. It is as
follows:

- `templates/Organisations/<organisation-code>/<source-domain>/<data-type>/<resource type>/`: This directory contains the
  liquid templates that are being tested.
- `tests/Templates.Tests/input/Organisations/<organisation-code>/<source-domain>/<data-type>/<resource type>/`: This directory
  contains the sample input files for the tests. The file name should be a combination of the resource type and the
  number of the example. For example, `x26/pds-mesh/json/patient/Patient-01.json`.
- `tests/Templates.Tests/output/Organisations/<organisation-code>/<source-domain>/<data-type>/<resource type>/`: This directory
  will contain the output of the tests.

## Test flow

The `Template.Tests` project includes a test that retrieves a list of all input files and creates a test case for each
one. Each test case is using the `Integration` version of the API and uses its `DataHubFhirClient` to call
the `$convert-data` endpoint of the FHIR service. Following this, a template validation is performed against
the `$validate` endpoint. Only after these steps are completed is the result of `$convert-data` compared with the
expected output.

## Running the Tests

### Pre-requisites

#### Set Environment Variables

Before running the tests, make sure that you have a `.env` file similar to the `.env.template` file in the `/docker` directory.
you must fill the following variables:

- `AZURE_CLIENT_SECRET` : the secret of the service principal that has access to the Azure ACR repository.
- `TAG`: the tag of the Azure Container Registry image that the templates will be pushed to.

for example:

```dotenv
...
# Templates tag to be pushed to ACR
TAG=JoeyTemplates001
```

[For further details about Docker environment files see this link.](https://docs.docker.com/compose/environment-variables/env-file/)

#### Initialise Environment

Before running the tests, be sure to build and run the required docker containers using the following command:

```bash
docker-compose -f ./docker/docker-compose.yml up templates-pusher data-init -d --build --force-recreate
```

> **NOTE:** the `-d` (detached) flag is used to run the services in the background, if you want to see the logs of the services in the terminal, you can remove the `-d` flag.

the docker compose file will start a local fhir server with its required sql db,
and the `templates-pusher` service that will push the templates to the azure container registry,
and the `data-init` service that will initialize the fhir server with the required data and profiles.

The `templates-pusher` container is expected to run and then exit. To confirm if the templates were pushed, view the `templates-pusher` container logs. You can also verify that the specified tag exists in Azure ACR repository for `dev`, by checking the status in the Azure portal:

1. Open the Azure portal
1. Navigate to the ACR
1. Open the Repositories screen
1. Open the `dev` repository
1. Check your tag exists and that Last Modified date is as expected

##### Troubleshooting

If there are any issues, such as "az login" needed, or an "invalid client secret is provided", run `docker-compose build --no-cache` followed by `docker-compose up --force-recreate templates-pusher -d`.

### Run the Tests

Once the docker containers are running and healthy, you can run the tests using your IDE's test runner.
> Your IDE's test runner should discover the tests in test runtime since the test cases are dynamically built based on the input folder.

Alternatively, you can run the tests using the command line interface (CLI), which is also the method used in our
Continuous Integration (CI) process:

```bash
dotnet test tests/Templates.Tests/Templates.Tests.csproj -e DataHubFhirServer:TemplateImage=acrdexoss.azurecr.io/dev:MYTAG
```

> **NOTE:** The `-e` flag is used to pass environment variables to the test runner. The `DataHubFhirServer:TemplateImage`
> variable is used to override the image tag that the tests will use to run the templates. The `MYTAG` should be replaced
> with the tag of the image that the templates were pushed to in the previous step.

### Debug a Single Test

Since the test inputs are dynamically built based on the input directory, all tests will have to run. However, you can
debug a single test input by applying a conditional breakpoint in the test code. For example, you could use
`relativePath.Contains("A03-01")` to isolate a specific test input.

## Assumptions

The tests assume that the Docker services are running and that the `templates-pusher` and `data-init` have been started manually. It also assumes that the `.env` file has been created and filled with the required variables, as described in the "Pre-requisites" section.
