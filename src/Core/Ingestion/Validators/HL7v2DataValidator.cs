using Core.Ingestion.Utilities;

namespace Core.Ingestion.Validators;

public static class HL7v2DataValidator
{
    private const string HeaderSegmentId = "MSH";
    private const char EscapeCharacter = '\\';

    public static bool ValidateMessageHeader(string? message)
    {

        var headerSegment = HL7v2Utility.SplitMessageToSegments(message!).FirstOrDefault();
        return !string.IsNullOrWhiteSpace(headerSegment) &&
               headerSegment.Length >= HeaderSegmentId.Length &&
               headerSegment.StartsWith(HeaderSegmentId, StringComparison.InvariantCultureIgnoreCase) &&
               headerSegment.Length >= 8 &&
               headerSegment.Substring(HeaderSegmentId.Length, 5).Distinct().Count() == 5 &&
               headerSegment[6] == EscapeCharacter &&
               headerSegment.Split('|').Length >= 9;
    }
}