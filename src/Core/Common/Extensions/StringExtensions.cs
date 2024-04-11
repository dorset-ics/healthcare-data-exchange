namespace Core.Common.Extensions;

public static class StringExtensions
{
    private const string crlf = "\r\n";
    private const string cr = "\r";
    private const string lf = "\n";

    public static string[] SplitLines(this string value)
    {
        return value.Split(new string[] { crlf, cr, lf }, StringSplitOptions.None);
    }
}