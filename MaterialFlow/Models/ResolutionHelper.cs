using System;

namespace MaterialFlow.Models;

public static class ResolutionHelper
{
    public static string GetAspectRatio(string resolution)
    {
        if (string.IsNullOrWhiteSpace(resolution)) return "Unknown";
        
        var parts = resolution.Split('x');
        if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
        {
            int gcd = GetGreatestCommonDivisor(w, h);
            return $"{w / gcd}:{h / gcd}";
        }
        return "Unknown";
    }

    private static int GetGreatestCommonDivisor(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }
}
