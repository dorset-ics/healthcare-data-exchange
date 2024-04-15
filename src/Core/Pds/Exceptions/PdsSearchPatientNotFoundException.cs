using System.Net;
using Hl7.Fhir.Rest;

namespace Core.Pds.Exceptions;

public class PdsSearchPatientNotFoundException(string message) : FhirOperationException(message, HttpStatusCode.NotFound);
