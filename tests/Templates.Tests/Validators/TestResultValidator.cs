using System.Text;
using JsonDiffPatchDotNet;
using Newtonsoft.Json.Linq;
using Templates.Tests.DataProviders;

namespace Templates.Tests.Validators;

public static class TestResultValidator
{
    internal static async Task CompareResponseWithExpectedFile(string relativePath, string responseContent)
    {
        var jsonFilePath = Path.ChangeExtension(relativePath, ".json");
        var outputFilePath = Path.Combine(TestPaths.OutputDirectoryPath, jsonFilePath);

        if (!File.Exists(outputFilePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath)!);
            await File.WriteAllTextAsync(outputFilePath, responseContent);
            throw new Exception($"Expected output file {outputFilePath} did not exist. Created for review.");
        }

        await CompareContent(responseContent, outputFilePath);
    }

    private static async Task CompareContent(string convertedContent, string expectedContentFilePath)
    {
        var expectedContent = await File.ReadAllTextAsync(expectedContentFilePath);

        var (convertedJson, expectedJson)
            = ConvertToComparableJson(convertedContent, expectedContent);

        var jdp = new JsonDiffPatch();
        var patch = jdp.Diff(convertedJson, expectedJson);

        patch.ShouldBeNull($"Differences found in {Path.GetFileNameWithoutExtension(expectedContentFilePath)}: Expected JSON: {expectedJson}, Converted JSON: {convertedJson}");
    }

    private static (JToken, JToken) ConvertToComparableJson(string convertedContent, string expectedContent)
    {
        var convertedJson = JToken.Parse(convertedContent);
        var expectedJson = JToken.Parse(expectedContent);

        RemoveIgnoredFields(convertedJson, expectedJson);

        return (convertedJson, expectedJson);
    }

    private static void RemoveIgnoredFields(JToken convertedJson, JToken expectedJson)
    {
        List<string> ignoreFields =
                [
                    "$.entry[?(@.resource.resourceType=='Provenance')].resource.text.div"
                ];

        ignoreFields.ForEach(path =>
        {
            convertedJson.SelectToken(path)?.Parent?.Remove();
            expectedJson.SelectToken(path)?.Parent?.Remove();
        });
    }
}