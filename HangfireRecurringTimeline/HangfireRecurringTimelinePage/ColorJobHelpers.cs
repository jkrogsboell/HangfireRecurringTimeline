using System.Drawing;

namespace HangfireRecurringTimelinePage;

internal static class ColorJobHelpers
{
    internal static string GetBackgroundColorString(string jobId)
    {
        var color = GetColorFromJobId(jobId);

        return $"rgba({color.R}, {color.G}, {color.B}, {color.A})";
    }

    internal static string GetTextColorString(string jobId)
    {
        var color = GetBlackOrWhiteColor(GetColorFromJobId(jobId));

        return $"rgba({color.R}, {color.G}, {color.B}, {color.A})";
    }

    private static Color GetColorFromJobId(string jobId)
    {
        var allColors = Enum.GetValues<KnownColor>();

        var rnd = new Random(jobId.GetHashCode());

        var index = rnd.Next() % allColors.Length;

        return Color.FromKnownColor(allColors[index]);
    }

    private static Color GetBlackOrWhiteColor(Color bg)
    {
        const int nThreshold = 105;
        var bgDelta = Convert.ToInt32((bg.R * 0.299) + (bg.G * 0.587) +
                                      (bg.B * 0.114));

        var foreColor = (255 - bgDelta < nThreshold) ? Color.Black : Color.White;
        return foreColor;
    }
}