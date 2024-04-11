FROM mcr.microsoft.com/azure-cli

ARG ACR_NAME
ARG AZURE_CLIENT_ID
ARG AZURE_CLIENT_SECRET
ARG AZURE_TENANT_ID
ENV AZURE_CLIENT_ID=$AZURE_CLIENT_ID AZURE_CLIENT_SECRET=$AZURE_CLIENT_SECRET AZURE_TENANT_ID=$AZURE_TENANT_ID ACR_NAME=$ACR_NAME
ENV ORAS_VERSION="1.1.0"

RUN wget "https://github.com/oras-project/oras/releases/download/v${ORAS_VERSION}/oras_${ORAS_VERSION}_linux_amd64.tar.gz"
RUN mkdir -p oras-install/
RUN tar -zxf oras_${ORAS_VERSION}_*.tar.gz -C oras-install/
RUN mv oras-install/oras /usr/local/bin/
RUN rm -rf oras_${ORAS_VERSION}_*.tar.gz oras-install/

COPY . templates
WORKDIR templates
RUN chmod +x flatten-templates.sh
RUN /bin/bash flatten-templates.sh
RUN rm -rf flatten-templates.sh

WORKDIR ..
RUN tar -czvf templates.tar.gz -C templates .
CMD az login --service-principal -u $AZURE_CLIENT_ID -p $AZURE_CLIENT_SECRET --tenant $AZURE_TENANT_ID && \
oras login ${ACR_NAME} \
--username 00000000-0000-0000-0000-000000000000 \
--password $(az acr login --name ${ACR_NAME} --expose-token --output tsv --query accessToken) && \
oras push ${ACR_NAME}/dev:${TAG:=$(cat /proc/sys/kernel/random/uuid)} templates.tar.gz;