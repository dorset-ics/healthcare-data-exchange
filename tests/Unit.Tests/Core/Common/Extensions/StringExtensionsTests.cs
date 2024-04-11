using Core.Common.Extensions;
using Hl7.Fhir.Model;

namespace Unit.Tests.Core.Common.Extensions;

public class StringExtensionsTests
{
    [Fact]
    public void SplitLines_WithCrLf_SplitsAsExpected()
    {
        var value = "Line 1\r\nLine 2\r\nLine 3";

        var lines = value.SplitLines();

        lines.Length.ShouldBe(3);
        lines[0].ShouldBe("Line 1");
        lines[1].ShouldBe("Line 2");
        lines[2].ShouldBe("Line 3");
    }

    [Fact]
    public void SplitLines_WithLf_SplitsAsExpected()
    {
        var value = "Line 1\nLine 2\nLine 3";

        var lines = value.SplitLines();

        lines.Length.ShouldBe(3);
        lines[0].ShouldBe("Line 1");
        lines[1].ShouldBe("Line 2");
        lines[2].ShouldBe("Line 3");
    }

    [Fact]
    public void SplitLines_WithCr_SplitsAsExpected()
    {
        var value = "Line 1\rLine 2\rLine 3";

        var lines = value.SplitLines();

        lines.Length.ShouldBe(3);
        lines[0].ShouldBe("Line 1");
        lines[1].ShouldBe("Line 2");
        lines[2].ShouldBe("Line 3");
    }

    [Fact]
    public void SplitLines_WithMixtureOfCrAndLf_SplitsAsExpected()
    {
        var value = "Line 1\rLine 2\nLine 3\r\nLine 4";

        var lines = value.SplitLines();

        lines.Length.ShouldBe(4);
        lines[0].ShouldBe("Line 1");
        lines[1].ShouldBe("Line 2");
        lines[2].ShouldBe("Line 3");
        lines[3].ShouldBe("Line 4");
    }

    [Fact]
    public void SplitLines_WithNoLineFeeds_SplitsAsExpected()
    {
        var value = "Line 1";

        var lines = value.SplitLines();

        lines.Length.ShouldBe(1);
        lines[0].ShouldBe("Line 1");
    }
}