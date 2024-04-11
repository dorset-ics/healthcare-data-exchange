namespace E2E.Tests.Common;

public record MeshClientsSettings(string MailboxId, string MailboxPassword, string Key, string WorkflowId, string? RecipientMailboxId = null);