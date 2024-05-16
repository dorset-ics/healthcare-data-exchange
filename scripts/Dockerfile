FROM mcr.microsoft.com/dotnet/sdk@sha256:64db914029faa026ff41d9284fa3badf039f94292ff1e5ba91e4b4b76e090968 # v7.0

RUN apt update && apt-get install jq curl bash

RUN dotnet tool install --global uploadfig 	

COPY . .

ENV PATH="/root/.dotnet/tools:${PATH}"

RUN chmod +x organisations/organisation-pusher.sh

CMD ["bash", "-c", "UploadFIG -d $FHIR_SERVER_URL -pid $PACKAGE_ID -vrd --includeReferencedDependencies -pv $PACKAGE_VERSION -gs true -cn -df json -dh \"Authorization:Bearer $FHIR_SERVER_AUTH_TOKEN\" && \
                    organisations/organisation-pusher.sh $FHIR_SERVER_URL $FHIR_SERVER_AUTH_TOKEN"]
