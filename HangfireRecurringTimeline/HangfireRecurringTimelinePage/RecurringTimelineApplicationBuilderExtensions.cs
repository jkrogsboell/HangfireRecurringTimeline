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

        DashboardRoutes.Routes.AddRazorPage(options.RelativePath, match => new HangfireRecurringTimelinePage(options.ViewType));

        if (options.AddMenuItem)
            NavigationMenu.Items.Add(page => new MenuItem(options.MenuItemName, page.Url.To(StartPath(options.RelativePath, options.ViewType)))
            {
                Active = page.RequestPath.StartsWith(options.RelativePath),
            });

        return app;
    }

    private static string StartPath(string relativePath, ViewType viewType)
    {
        if (viewType == ViewType.Week)
            return relativePath;
        
        return $"{relativePath}?date={DateTime.Today:yyyy-MM-dd}";
    }

    public class RecurringTimelineOptions
    {
        public string RelativePath { get; set; } = "/timeline-recurringjobs";
        public bool AddMenuItem { get; set; } = true;
        public string MenuItemName { get; set; } = "Recurring Timeline";
        public ViewType ViewType { get; set; } = ViewType.Week;
    }

    public enum ViewType
    {
        Week,
        Day
    }
}