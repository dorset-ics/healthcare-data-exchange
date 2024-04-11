set -o xtrace
#!/bin/bash

# !!! ENSURE YOU HAVE CONFIGURED YOUR .env !!! 
# e.g. cp .env.template .env

echo "Resetting docker environment..."

docker-compose stop

rm -r -f -- ./mounted-data/*/


# Start docker env

echo "Starting docker environment..."

docker-compose up -d --build --force-recreate


# Wait for fhir-api to be healthy and available

timeout=120
interval=5
elapsed=0

echo "Waiting for fhir-api to be healthy..."

while [[ "$(curl -s -o /dev/null -w '%{http_code}' localhost:8080/metadata)" != "200" && $elapsed -lt $timeout ]]; do
    echo "Still waiting... Elapsed time: $elapsed seconds"
    sleep $interval
    ((elapsed+=interval))
done

if [ $elapsed -ge $timeout ]; then
    echo "Timed out waiting for fhir-api to be healthy. Exiting."
    docker-compose -f docker-compose.yml logs
    docker-compose -f docker-compose.yml ps
    exit 1
fi

echo "fhir-api is healthy and available!"


# Run data-init

echo "Initialising data..."

docker-compose build data-init

docker-compose run data-init


# Run templates pusher

echo "Pushing templates..."

docker-compose build templates-pusher

docker-compose run templates-pusher