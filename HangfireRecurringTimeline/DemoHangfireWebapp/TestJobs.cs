using Hangfire;
using Hangfire.Server;

namespace DemoHangfireWebapp;

public class TestJobs
{
    public async Task TestJobRecurring()
    {
        await Task.Delay(12300);
    }

    public async Task TestJobRecurringWithContinuation(PerformContext performContext)
    {
        await Task.Delay(1233);

        BackgroundJob.ContinueJobWith(performContext.BackgroundJob.Id, () => TestWithContinuation(null!));
    }

    public async Task TestWithContinuation(PerformContext performContext)
    {
        await Task.Delay(1233);
        BackgroundJob.ContinueJobWith(performContext.BackgroundJob.Id, () => TestJobSimple());
    }

    public async Task TestJobSimple()
    {
        await Task.Delay(1233);
    }
}