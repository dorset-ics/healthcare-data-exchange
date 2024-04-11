namespace Infrastructure.DataHub.Models;
public class Parameter
{
    public string Name { get; set; } = null!;

    public string ValueString { get; set; } = null!;
}
public class ConvertPayload(
    string inputData,
    string inputDataType,
    string templateCollectionReference,
    string rootTemplate)
{
    public string ResourceType { get; set; } = "Parameters";

    public Parameter[] Parameter { get; set; } =
    {
        new Parameter { Name = "inputData", ValueString = inputData },
        new Parameter { Name = "inputDataType", ValueString = inputDataType },
        new Parameter { Name = "templateCollectionReference", ValueString = templateCollectionReference },
        new Parameter { Name = "rootTemplate", ValueString = rootTemplate }
    };
}