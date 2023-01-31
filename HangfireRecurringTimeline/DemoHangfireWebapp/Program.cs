using DemoHangfireWebapp;
using Hangfire;
using Hangfire.MemoryStorage;
using HangfireRecurringTimelinePage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHangfire(config =>
{
    config.UseMemoryStorage();
});
builder.Services.AddHangfireServer();

var app = builder.Build();

app
    .UseHangfireDashboard()
    .UseRecurringTimeline(new RecurringTimelineApplicationBuilderExtensions.RecurringTimelineOptions()
    {
        MenuItemName = "Timeline Day",
        ViewType = RecurringTimelineApplicationBuilderExtensions.ViewType.Day,
        AddMenuItem = true,
        RelativePath = "/timeline_day"
    })
    .UseRecurringTimeline(new RecurringTimelineApplicationBuilderExtensions.RecurringTimelineOptions()
    {
        MenuItemName = "Timeline Week",
        ViewType = RecurringTimelineApplicationBuilderExtensions.ViewType.Week,
        AddMenuItem = true,
        RelativePath = "/timeline_week"
    });
    ;

RecurringJob.AddOrUpdate<TestJobs>("Id_of_Recurring", (job) => job.TestJobRecurring(), "0/5 13,18 * * *");
RecurringJob.AddOrUpdate<TestJobs>("Id_of_RecCont", (job) => job.TestJobRecurringWithContinuation(null!), "23 0-18 * * *");

RecurringJob.TriggerJob("Id_of_Recurring");
RecurringJob.TriggerJob("Id_of_RecCont");

app.Run();