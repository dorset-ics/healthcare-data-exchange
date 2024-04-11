using Hl7.Fhir.Model;

namespace Core.Common.Exceptions;

public class ValidationOperationOutcomeException(OperationOutcome operationOutcome) : Exception
{
    public OperationOutcome OperationOutcome { get; } = operationOutcome;
}