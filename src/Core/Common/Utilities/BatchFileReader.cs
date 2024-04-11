using System.Text;

namespace Core.Common.Utilities;

public class BatchFileReader(Stream stream, int batchSize) : IDisposable
{
    private readonly StreamReader _reader = new StreamReader(stream);

    public IEnumerable<string> ReadBatches()
    {
        var stringBuilder = new StringBuilder();
        while (_reader.Peek() >= 0)
        {
            for (var i = 0; i < batchSize && _reader.Peek() >= 0; i++)
            {
                var line = _reader.ReadLine();
                if (line != "\n" && !string.IsNullOrWhiteSpace(line))
                {
                    stringBuilder.AppendLine(line);
                }
            }

            if (stringBuilder.Length == 0)
            {
                yield break;
            }

            yield return stringBuilder.ToString();
            stringBuilder.Clear();
        }
    }

    public void Dispose()
    {
        _reader.Dispose();
        GC.SuppressFinalize(this);
    }
}