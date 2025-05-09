using BobsBBQApi.Services;

namespace BobsBBQApi.Helpers;

public class DefaultTimeSlots
{
    public static List<DateTime> GetSlots(DateTime date)
    {
        MonitorService.Log.Information("Getting default time slots for date: {@date}", date);
        return Enumerable.Range(11, 10)  // 11 to 20 (inclusive) for hours
            .Select(hour => date.Date.AddHours(hour)) 
            .ToList();
    }
}