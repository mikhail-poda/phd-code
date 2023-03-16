using System;

namespace Library;

public static class Utils
{
    public static Range SafeRange(int index, int leftOffset, int rightOffset = 0, int maxLen = int.MaxValue)
    {
        return new Range(Math.Max(0, index - leftOffset), Math.Min(index + rightOffset, maxLen));
    }
    
    public static bool IsPrintable(char @char)
    {
        if (@char == '\r') return true;
        if (@char == '\n') return true;

        if (@char < ' ') return false;
        if (@char <= '~') return true;

        if (@char == 'ü') return true;
        if (@char == 'ö') return true;
        if (@char == 'ä') return true;
        if (@char == 'ß') return true;
        if (@char == 'Ä') return true;
        if (@char == 'Ö') return true;
        if (@char == 'Ü') return true;

        return false;
    }

    public static string ToSingleLine(string text)
    {
        return text
            .Replace('\n', ' ')
            .Replace('\r', ' ')
            .Replace('>', ' ')
            .Replace("     ", " ")
            .Replace("    ", " ")
            .Replace("   ", " ")
            .Replace("  ", " ");
    }
}