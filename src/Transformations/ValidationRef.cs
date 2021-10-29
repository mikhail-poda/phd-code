using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Library;

namespace PhdCode;

public enum ValidationType
{
    NoRef,
    NoYear,
    Valid,
    Invalid
}

public record ValidationResult
{
    public IBibItem BibItem { get; init; }
    public ValidationType ValType { get; init; }
}

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
            var list = matches.Select(x => Validate2(prefixes, text, x, items)).ToList();

            Console.WriteLine("------------------- Valid -------------------");
            foreach (var match in list.Where(x => x.Item1.ValType == ValidationType.Valid))
                Console.WriteLine(match.Item2);

            Console.WriteLine("------------------- NoYear -------------------");
            foreach (var match in list.Where(x => x.Item1.ValType == ValidationType.NoYear))
                Console.WriteLine(match.Item2);

            Console.WriteLine("------------------- NoRef -------------------");
            foreach (var match in list.Where(x => x.Item1.ValType == ValidationType.NoRef))
                Console.WriteLine(match.Item2);

            Console.WriteLine("------------------- Invalid -------------------");
            foreach (var match in list.Where(x => x.Item1.ValType == ValidationType.Invalid))
                Console.WriteLine(match.Item2);
        }
    }

    private static Tuple<ValidationResult, string> Validate2(IEnumerable<string> prefixes, string text, Match match,
        Dictionary<int, List<IBibItem>> items)
    {
        var result = Validate(prefixes, text, match, items);
        var range = new Range(Math.Max(0, match.Index - 50), match.Index + 4);
        var kvp = new Tuple<ValidationResult, string>(result, text[range]);

        return kvp;
    }

    private static ValidationResult Validate(IEnumerable<string> prefixes, string text, Capture match,
        IDictionary<int, List<IBibItem>> items)
    {
        var hasPrefix = prefixes
            .Select(prefix => CompareBackwards(text, match.Index, prefix))
            .Any(x => x);

        // e.g. im Jahr 2013
        if (hasPrefix)
            return new ValidationResult {ValType = ValidationType.NoRef};

        var ind = match.Index;

        // e.g. 12.12.2013
        var isDate = ind > 6 && DateTime.TryParseExact(
            text[(ind - 6) .. (ind + 4)],
            "dd.MM.yyyy",
            _format,
            DateTimeStyles.None,
            out _);
        if (isDate) return new ValidationResult {ValType = ValidationType.NoRef};

        // e.g. 12/2345 or =1234 or 2.1234 or 012345 or 
        var isNumber =
            text[ind - 1] == '/' ||
            text[ind - 1] == '=' ||
            (text[ind - 1] == '.' && char.IsDigit(text[ind - 2])) ||
            char.IsDigit(text[ind - 1]) ||
            char.IsDigit(text[ind - 5]);
        if (isNumber) return new ValidationResult {ValType = ValidationType.NoRef};

        if (text[ind - 1] == '(') ind -= 1;

        var year = int.Parse(match.Value);
        var group = items.TryGetValue(year);
        if (group == null) return new ValidationResult {ValType = ValidationType.NoYear};

        foreach (var item in @group)
        {
            var list = item.Authors.Select(x => x.Key).ToList();
            var authors = string.Join(", ", list);
            if (authors.IsNullOrEmpty()) continue;

            var isValid = CompareBackwards(text, ind, authors);
            if (isValid) return new ValidationResult {ValType = ValidationType.Valid, BibItem = item};

            if (list.Count == 2)
            {
                authors = list[0] + "/" + list[1];
                isValid = CompareBackwards(text, ind, authors);
                if (isValid) return new ValidationResult {ValType = ValidationType.Valid, BibItem = item};
            }
            else if (list.Count > 2)
            {
                authors = list[0] + " et al.";
                isValid = CompareBackwards(text, ind, authors);
                if (isValid) return new ValidationResult {ValType = ValidationType.Valid, BibItem = item};
            }
        }

        return new ValidationResult {ValType = ValidationType.Invalid};
    }

    private static bool CompareBackwards(string text, int index, string str)
    {
        var range = new Range(index - 1 - str.Length, index - 1);
        return text[range] == str;
    }
}