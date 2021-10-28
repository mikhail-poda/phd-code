using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Library;

namespace PhdCode;

public static class ValidationRef
{
    private static readonly IFormatProvider _format = DateTimeFormatInfo.InvariantInfo;

    public static void Validate()
    {
        var risPath = @"D:\code\Mikhail\phd-private\bibliography\bibliography.ris";
        var items = new RisBibliography(risPath).Items
            .GroupBy(x => x.Year)
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(y => y.Authors.Count).ToList());

        var folder = @"D:\code\Mikhail\phd-private\md";
        var files = Directory.EnumerateFiles(folder, "*.md");
        var count = 0;
        Range range;
        var prefixes = File.ReadAllLines("IgnoreNames.txt");

        foreach (var file in files)
        {
            Console.WriteLine(file);

            var text = File
                .ReadAllText(file)
                .Replace('\n', ' ')
                .Replace('\r', ' ')
                .Replace('>', ' ')
                .Replace("     ", " ")
                .Replace("    ", " ")
                .Replace("   ", " ")
                .Replace("  ", " ");

            var matches = Regex.Matches(text, "\\d{4}");
            var len = text.Length;

            foreach (Match match in matches)
            {
                var isValid = Validate(prefixes, text, match, items);
                if (isValid) continue;

                range = new Range(Math.Max(0, match.Index - 50), match.Index + 4);
                Console.WriteLine(text[range] + "         " + isValid);
                count++;
            }
        }

        Console.WriteLine(count);
    }

    private static bool Validate(IEnumerable<string> prefixes, string text, Capture match,
        IDictionary<int, List<IBibItem>> items)
    {
        var hasPrefix = prefixes
            .Select(prefix => CompareBackwards(text, match.Index, prefix))
            .Any(x => x);

        if (hasPrefix)
            return true;

        var ind = match.Index;

        var isDate = ind > 6 && DateTime.TryParseExact(
            text[(ind - 6) .. (ind + 4)],
            "dd.MM.yyyy",
            _format,
            DateTimeStyles.None,
            out _);
        if (isDate) return true;

        var isNumber =
            text[ind - 1] == '/' ||
            text[ind - 1] == '=' ||
            (text[ind - 1] == '.' && char.IsDigit(text[ind - 2])) ||
            char.IsDigit(text[ind - 1]) ||
            char.IsDigit(text[ind - 5]);
        if (isNumber) return true;

        if (text[ind - 1] == '(') ind -= 1;

        var year = int.Parse(match.Value);
        var group = items.TryGetValue(year);
        if (group == null) return false;

        foreach (var item in @group)
        {
            var list = item.Authors.Select(x => x.Key).ToList();
            var authors = string.Join(", ", list);
            if (authors.IsNullOrEmpty()) continue;

            var isValid = CompareBackwards(text, ind, authors);
            if (isValid) return true;

            if (list.Count == 2)
            {
                authors = list[0] + "/" + list[1];
                isValid = CompareBackwards(text, ind, authors);
                if (isValid) return true;
            }
            else if (list.Count > 2)
            {
                authors = list[0] + " et al.";
                isValid = CompareBackwards(text, ind, authors);
                if (isValid) return true;
            }
        }

        return false;
    }

    private static bool CompareBackwards(string text, int index, string str)
    {
        var range = new Range(index - 1 - str.Length, index - 1);
        return text[range] == str;
    }
}