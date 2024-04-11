namespace Core.Common.Abstractions.Services;

public interface ITerminologyService
{
    string GetSnomedDisplay(string code);
}