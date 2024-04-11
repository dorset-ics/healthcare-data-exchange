namespace Core.Ingestion.Utilities;

public static class HL7v2Utility
{
    private static readonly string[] SegmentSeparators = { "\r\n", "\r", "\n" };

    public static string[] SplitMessageToSegments(string message)
    {
        var segments = message.Split(SegmentSeparators, StringSplitOptions.RemoveEmptyEntries);
        return segments;
    }

    public static string GetMessageType(string message)
    {
        var pipes = message.Split('|');
        var messageContents = pipes[8].Split('^');
        var codeAndEvent = messageContents[..2];
        return string.Join(string.Empty, codeAndEvent);
    }

    public static string GetMessageControlId(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return string.Empty;
        }

        var splitMessage = message.Split('|');
        return splitMessage.Length > 9 ? splitMessage[9] : string.Empty;
    }
}