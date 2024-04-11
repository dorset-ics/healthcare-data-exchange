namespace Infrastructure.Ndop.Mesh.Configuration;

public record NdopMeshConfiguration(string SendSchedule,
    string RetrieveSchedule,
    string MailboxId,
    string MailboxPassword,
    string Key,
    string WorkflowId,
    string RecipientMailboxId
    )
{
    public const string SectionKey = "Ndop:Mesh";
}

