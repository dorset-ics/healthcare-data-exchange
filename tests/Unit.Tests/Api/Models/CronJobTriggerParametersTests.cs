using Api;

namespace Unit.Tests.Api.Models;

public class CronJobTriggerParametersTests
{
    [Fact]
    public void Properties_ShouldBeSetCorrectly()
    {
        const string jobKey = "JobKey";
        const string jobDescription = "JobDescription";
        const string triggerKey = "TriggerKey";
        const string triggerDescription = "TriggerDescription";
        const string cronSchedule = "CronSchedule";

        var parameters = new DependencyInjection.CronJobTriggerParameters(jobKey, jobDescription, triggerKey, triggerDescription, cronSchedule);

        parameters.JobKey.ShouldBe(jobKey);
        parameters.JobDescription.ShouldBe(jobDescription);
        parameters.TriggerKey.ShouldBe(triggerKey);
        parameters.TriggerDescription.ShouldBe(triggerDescription);
        parameters.CronSchedule.ShouldBe(cronSchedule);
    }
}