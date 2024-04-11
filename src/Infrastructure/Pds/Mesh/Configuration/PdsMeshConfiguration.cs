namespace Infrastructure.Pds.Mesh.Configuration;

public record PdsMeshConfiguration(string SendSchedule, string RetrieveSchedule, string MailboxId, string MailboxPassword, string Key, string WorkflowId)
{
    public const string SectionKey = "Pds:Mesh";
}