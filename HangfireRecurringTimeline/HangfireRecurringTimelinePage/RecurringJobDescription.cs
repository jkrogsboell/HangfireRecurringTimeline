namespace HangfireRecurringTimelinePage;

internal class RecurringJobDescription
{
    public string CronExpression { get; init; } = null!;
    public string CronExplanation { get; init; } = null!;
    public string RecurringJobId { get; init; } = null!;
    public string Queue { get; init; } = null!;
    public DateTime StartTime { get; init; }
    public int? LastDuration { get; init; }
    public string? JobId { get; init; }
    public int ContinuationCount { get; init; }
    public string Name { get; init; } = null!;
}