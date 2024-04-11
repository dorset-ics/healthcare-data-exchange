namespace Unit.Tests.Api.Utilities;

public class HL7v2Regex
{
    internal const string HL7v2MessageHeaderPattern = @"MSH\|\^~\\&\|DEX\|\|\|\|\d{14}\|\|ACK\|[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}\|P\|2\.4";
}