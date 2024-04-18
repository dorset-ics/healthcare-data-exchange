# Development Guidelines

Before you start developing, please read the following guidelines.

## Data Initialisation

In order to have data in your FHIR server when it comes up, you will need to run the data initialization which includes:

1. Insertion of FHIR profiles - upload a FHIR implementation guide profile package to a FHIR server
1. Insertion of ICS organisations data - insert the ICS organisations jsons in the `/scripts/organisations/data` directory.

To do so, you can run the data-init container with the FHIR server URL as an argument:

```bash
FHIR_SERVER_URL=http://localhost:8080/ docker-compose -f docker/docker-compose.yml up data-init
```

You can also specify the package name and version as arguments to override defaults:

```bash
FHIR_SERVER_URL=http://localhost:8080/ PACKAGE_ID=fhir.r4.ukcore.stu3.currentbuild PACKAGE_VERSION=0.0.8-pre-release docker-compose -f docker/docker-compose.yml up data-init
```

Or supply a server token if the server requires authentication:

```bash
FHIR_SERVER_URL=http://localhost:8080/ FHIR_SERVER_TOKEN=<auth-bearer-token> docker-compose -f docker/docker-compose.yml up data-init
```

This container is intended to be used locally for development purposes, like testing the validity of converted resources,
and also for inserting FHIR data to a FHIR server in a CD pipeline, on a remote FHIR server.

## Liquid Templates

The liquid templates which define how source data should be converted into FHIR are stored in the `./templates` directory of the repository. In order for data conversion operations to execute you need to build a docker container containing the templates, and push it to Azure Container Registry.

A docker image is supplied which performs this task.

First set the tag which will be applied to the container in the `.env` file, within the `./docker` directory.
You also need to update your local appsettings with the tag value before run running the solution, and running the tests.

Next build the container with the following command:

`docker-compose -f docker/docker-compose.yml build templates-pusher`

And finally run the container with the following command, which will push the container to the ACR:

`docker-compose -f docker/docker-compose.yml run templates-pusher`

> :information_source: You need to rebuild and run the container if any changes are made to the templates.

## Running the application with its dependencies

Alternatively to running each service individually (as described above), you can run all the required services using the script `start-clean.sh`:

```bash
bash ./docker/start-clean.sh
```

This process is also described in the [Getting Started](../getting-started.md) guide and it will not only run the data initialization and the templates pusher, but also the FHIR server, the SQL Server database for the FHIR server, and the Azurite Azure Storage Emulator.
