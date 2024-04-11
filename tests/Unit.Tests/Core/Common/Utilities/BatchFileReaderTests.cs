using Core.Common.Utilities;

namespace Unit.Tests.Core.Common.Utilities;

public class BatchFileReaderTests
{
    [Fact]
    public void ReadBatches_WhenCalled_ReturnsBatches()
    {
        var filePath = "test.txt";
        var batchSize = 3;
        string[] lines = { "line1", "line2", "line3", "line4", "line5" };
        File.WriteAllText(filePath, string.Join(Environment.NewLine, lines));
        using var stream = new StreamReader(filePath);
        using var reader = new BatchFileReader(stream.BaseStream, batchSize);

        var batches = reader.ReadBatches();

        Assert.Equal(
            new string[]
            {
                "line1" + Environment.NewLine + "line2" + Environment.NewLine + "line3" + Environment.NewLine,
                "line4" + Environment.NewLine + "line5" + Environment.NewLine
            }, batches);
    }

    [Fact]
    public void ReadBatches_WhenFileIsEmpty_ReturnsEmptyBatches()
    {
        var filePath = "test.txt";
        File.WriteAllText(filePath, string.Empty);
        using var stream = new StreamReader(filePath);
        using var reader = new BatchFileReader(stream.BaseStream, 3);

        var batches = reader.ReadBatches();

        Assert.False(batches.Any());
    }

    [Fact]
    public void ReadBatches_WhenFileContainsLessLinesThanBatchSize_ReturnsSingleBatch()
    {
        var filePath = "test.txt";
        var lines = new string[] { "line1", "line2" };
        File.WriteAllText(filePath, string.Join(Environment.NewLine, lines));
        using var stream = new StreamReader(filePath);
        using var reader = new BatchFileReader(stream.BaseStream, 3);

        var batches = reader.ReadBatches();

        Assert.Equal(new string[] { "line1" + Environment.NewLine + "line2" + Environment.NewLine }, batches);
    }

    [Fact]
    public void ReadBatches_WhenFileContainsEmptyLines_ReturnsSingleBatch()
    {
        var filePath = "test.txt";
        var lines = new string[] { "line1", "line2", Environment.NewLine, Environment.NewLine };
        File.WriteAllText(filePath, string.Join(Environment.NewLine, lines));
        using var stream = new StreamReader(filePath);
        using var reader = new BatchFileReader(stream.BaseStream, 3);

        var batches = reader.ReadBatches();

        Assert.Equal(new string[] { "line1" + Environment.NewLine + "line2" + Environment.NewLine }, batches);
    }

    [Fact]
    public void ReadBatches_WhenFileContainsEmptyString_ReturnsSingleBatch()
    {
        var filePath = "test.txt";
        var lines = new string[] { "line1", "line2", "", Environment.NewLine, Environment.NewLine };
        File.WriteAllText(filePath, string.Join(Environment.NewLine, lines));
        using var stream = new StreamReader(filePath);
        using var reader = new BatchFileReader(stream.BaseStream, 3);

        var batches = reader.ReadBatches();

        Assert.Equal(new string[] { "line1" + Environment.NewLine + "line2" + Environment.NewLine }, batches);
    }
}