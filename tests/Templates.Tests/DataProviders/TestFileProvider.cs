using Core.Common.Models;

namespace Templates.Tests.DataProviders;

internal sealed class TestFileProvider
{
    public static TheoryData<string, TemplateInfo> GetTestInputFiles()
    {
        var data = new TheoryData<string, TemplateInfo>();

        foreach (var file in Directory.EnumerateFiles(TestPaths.InputDirectoryPath, "*.*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(TestPaths.InputDirectoryPath, file);
            var templateInfo = ExtractTemplateInfoFromTestFile(file);
            data.Add(relativePath, templateInfo);
        }

        return data;
    }

    private static TemplateInfo ExtractTemplateInfoFromTestFile(string inputFile)
    {
        var directoryInfo = new DirectoryInfo(inputFile).Parent;
        var resourceType = directoryInfo?.Name!;

        directoryInfo = directoryInfo?.Parent;
        var dataType = directoryInfo?.Name!;

        directoryInfo = directoryInfo?.Parent;
        var domain = directoryInfo?.Name!;

        var organisation = directoryInfo?.Parent?.Name!;

        return new TemplateInfo(organisation, domain, dataType, resourceType);
    }
}