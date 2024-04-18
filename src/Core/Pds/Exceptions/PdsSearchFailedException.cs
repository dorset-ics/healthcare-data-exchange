using System.Net;
using Hl7.Fhir.Rest;

namespace Core.Pds.Exceptions;

public class PdsSearchFailedException(string message) : FhirOperationException(message, HttpStatusCode.BadRequest);
