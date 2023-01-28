namespace HangfireRecurringTimelinePage;

using Cronos;
using Hangfire;
using Hangfire.Common;
using Hangfire.Dashboard;
using Hangfire.Dashboard.Pages;
using Hangfire.States;
using Hangfire.Storage;
using Newtonsoft.Json;

public class HangfireRecurringTimelinePage : LayoutPage
{
    private const int MaxDepth = 100;

    public HangfireRecurringTimelinePage() : base("Recurring Timeline")
    {
    }

    public override void Execute()
    {
        var initialDay = DateTime.Today;

        var recurringJobs = base.Storage.GetConnection().GetRecurringJobs();

        var dow = (int)initialDay.DayOfWeek;

        if (dow == 0)
            dow = 7;

        var startDate = initialDay.AddDays(-dow + (int)DayOfWeek.Monday);
        ;

        var recurringJobDescriptions = CalculateDescriptions(recurringJobs, startDate.ToUniversalTime(),
            startDate.AddDays(7).ToUniversalTime());

        var events =
            recurringJobDescriptions
                .Select(rjd => new
                {
                    title = rjd.RecurringJobId,
                    start = rjd.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = rjd.StartTime.AddMilliseconds(rjd.LastDuration ?? 60000).ToString("yyyy-MM-ddTHH:mm:ss"),
                    description = HangfireRecurringTimelineHtmlGenerator.GetPopupDescription(rjd),
                    textColor = ColorJobHelpers.GetTextColorString(rjd.RecurringJobId),
                    backgroundColor = ColorJobHelpers.GetBackgroundColorString(rjd.RecurringJobId)
                })
                .Select(x => JsonConvert.SerializeObject(x, Formatting.Indented))
                .ToList();

        var html = HangfireRecurringTimelineHtmlGenerator.GeneratePageHtml(initialDay, events);

        var content = new NonEscapedString(html);

        Write(content);

        base.Execute();
    }

    private static List<Continuation> DeserializeContinuations(string serialized)
    {
        var continuations = SerializationHelper.Deserialize<List<Continuation>>(serialized);

        if (continuations != null && continuations.TrueForAll(x => x.JobId == null))
        {
            continuations =
                SerializationHelper.Deserialize<List<Continuation>>(serialized, SerializationOption.Internal);
        }

        return continuations ?? new List<Continuation>();
    }

    private struct Continuation
    {
        public string JobId { get; set; }
        public JobContinuationOptions Options { get; set; }
    }

    private void CalcMaxSucceededAt(ref DateTimeOffset? firstCreatedAt, ref DateTimeOffset? currentMax,
        ref int continuationCount,
        string continuationJobId, int depth)
    {
        if (depth > MaxDepth)
            return;

        var jobDetails = Storage.GetMonitoringApi().JobDetails(continuationJobId);

        if (jobDetails == null)
            return;

        firstCreatedAt ??= jobDetails.CreatedAt;

        var history = jobDetails.History;

        var succeededState = history?.FirstOrDefault(x => x.StateName == SucceededState.StateName);
        var succeededAt = succeededState?.Data["SucceededAt"];

        if (succeededAt == null)
            return;

        var succeedAtDt = (DateTimeOffset?)DateTimeOffset.Parse(succeededAt);

        if (succeedAtDt > currentMax || currentMax == null)
            currentMax = succeedAtDt;

        if (jobDetails.Properties.TryGetValue("Continuations", out var serialized))
        {
            var jobIds = DeserializeContinuations(serialized).Select(x => x.JobId).ToList();
            foreach (var jobId in jobIds)
            {
                continuationCount++;
                CalcMaxSucceededAt(ref firstCreatedAt, ref currentMax, ref continuationCount, jobId, depth++);
            }
        }
    }

    private List<RecurringJobDescription> CalculateDescriptions(RecurringJobDto recurringJobDto, DateTime start,
        DateTime end)
    {
        var cronExpression = CronExpression.Parse(recurringJobDto.Cron);

        var occurrences = cronExpression.GetOccurrences(start, end, TimeZoneInfo.Local);

        var description = CronExpressionDescriptor.ExpressionDescriptor.GetDescription(recurringJobDto.Cron);

        int? lastDurationInMillis = null;
        var continuationCount = 0;
        DateTimeOffset? succeededMax = null;
        DateTimeOffset? createdAt = null;
        if (recurringJobDto.LastJobId != null)
            CalcMaxSucceededAt(ref createdAt, ref succeededMax, ref continuationCount, recurringJobDto.LastJobId, 0);

        if (createdAt != null && succeededMax != null)
        {
            lastDurationInMillis = (int)(succeededMax.Value.Subtract(createdAt.Value)).TotalMilliseconds;
        }

        return occurrences.Select(
            occurence => new RecurringJobDescription()
            {
                Name = recurringJobDto.Job.ToString(),
                CronExplanation = description,
                CronExpression = recurringJobDto.Cron,
                StartTime = occurence.ToLocalTime(),
                JobId = recurringJobDto.LastJobId,
                Queue = recurringJobDto.Queue,
                RecurringJobId = recurringJobDto.Id,
                LastDuration = lastDurationInMillis,
                ContinuationCount = continuationCount
            }
        ).ToList();
    }

    private IReadOnlyList<RecurringJobDescription> CalculateDescriptions(
        List<RecurringJobDto> recurringJobDtos, DateTime start, DateTime end)
    {
        return recurringJobDtos
            .Where(x => !x.Removed)
            .SelectMany(x => CalculateDescriptions(x, start, end))
            .ToList();
    }
}