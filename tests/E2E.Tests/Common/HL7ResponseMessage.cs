namespace E2E.Tests.Common;

public class HL7ResponseMessage(string message)
{
    private const string Separator = "|";

    public string SourceDomain => message.Split(Separator)[4];
    public string Organisation => message.Split(Separator)[5];
    public bool IsAcknowledgement => message.Split(Separator)[8] == "ACK";
    public string AckCode => message.Split(Separator)[12];
    public string RequestMessageControlId => message.Split(Separator)[13];
    public string Message => message.Split(Separator)[14];
}