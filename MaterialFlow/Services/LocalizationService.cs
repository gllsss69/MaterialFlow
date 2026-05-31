using Avalonia;

namespace MaterialFlow.Services;

public static class LocalizationService
{
    public static string Get(string key, string fallback)
    {
        if (Application.Current?.Resources.TryGetResource(key, null, out var value) == true && value is string text)
        {
            return text;
        }

        return fallback;
    }
}
