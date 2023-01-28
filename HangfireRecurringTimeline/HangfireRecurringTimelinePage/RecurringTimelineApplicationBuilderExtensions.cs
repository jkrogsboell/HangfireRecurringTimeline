using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HangfireRecurringTimelinePage;

public static class RecurringTimelineApplicationBuilderExtensions
{
    public static IApplicationBuilder UseRecurringTimeline(
        [NotNull] this IApplicationBuilder app,
        [CanBeNull] RecurringTimelineOptions? options = null!)
    {
        var applicationServices = app.ApplicationServices;
        options = options ?? applicationServices.GetService<RecurringTimelineOptions>() ??
            new RecurringTimelineOptions();

        DashboardRoutes.Routes.AddRazorPage(options.RelativePath, match => new HangfireRecurringTimelinePage());

        if (options.AddMenuItem)
            NavigationMenu.Items.Add(page => new MenuItem(options.MenuItemName, page.Url.To(options.RelativePath))
            {
                Active = page.RequestPath.StartsWith(options.RelativePath),
            });

        return app;
    }

    public class RecurringTimelineOptions
    {
        public string RelativePath { get; set; } = "/timeline-recurringjobs";
        public bool AddMenuItem { get; set; } = true;
        public string MenuItemName { get; set; } = "Recurring Timeline";
    }
}