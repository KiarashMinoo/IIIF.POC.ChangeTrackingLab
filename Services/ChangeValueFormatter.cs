using Newtonsoft.Json;

namespace IIIF.POC.ChangeTrackingLab.Services;

public static class ChangeValueFormatter
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        NullValueHandling = NullValueHandling.Ignore
    };

    public static string ToStableJson(object? value)
    {
        if (value is null)
            return "null";

        try
        {
            return JsonConvert.SerializeObject(value, Formatting.None, Settings);
        }
        catch (JsonException)
        {
            return JsonConvert.SerializeObject(value.ToString());
        }
    }

    public static string ToDisplayValue(object? value, int maxLength = 180)
    {
        var json = ToStableJson(value);
        return json.Length <= maxLength ? json : json[..maxLength] + "…";
    }
}
