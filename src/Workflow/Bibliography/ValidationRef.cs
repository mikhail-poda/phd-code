using System.Globalization;
using System.Text.RegularExpressions;
using Library;

namespace Workflow.Bibliography;

public enum ValidationType
{
    NoRef, // is not a reference but date or number or piece of text
    NoYear, // year not found
    Valid, // ok, authors found with right format
    Format, // authors found with wrong format
    Invalid // none of the above
}

public record ValidationResult
{
    public IBibItem? BibItem { get; init; }
    public ValidationType ValType { get; init; }
}

public static class ValidationRef
{
    private static readonly IFormatProvider _format = DateTimeFormatInfo.InvariantInfo;

    public static void Validate()
    {
        var risPath = @"D:\code\phd-private\bibliography\bibliography.ris";
        var items = new RisBibliography(risPath).ToList();
        var yearItemsDict = items
            .GroupBy(x => x.Year)
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(y => y.Authors.Count).ToList());

        var itemCountDict = items.ToDictionary(x => x, x => 0);

        var folder = @"D:\code\phd-private\md";
        var files = Directory.EnumerateFiles(folder, "*.md");

        var prefixes = File
            .ReadAllLines("IgnoreNames.txt")
            .Select(x => x.Length <= 3 ? " " + x : x)
            .ToList();

        var list = files
            .Select(f => ValidateFile(f, prefixes, yearItemsDict))
            .SelectMany(x => x)
            .ToArray();

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

        Console.WriteLine("------------------- Format Issue -------------------");
        foreach (var match in list.Where(x => x.Item1.ValType == ValidationType.Format))
            Console.WriteLine(match.Item2);

        Console.WriteLine("------------------- Use -------------------");
        foreach (var match in list.Where(x => x.Item1.BibItem != null))
            itemCountDict[match.Item1.BibItem] += 1;

        foreach (var item in itemCountDict.OrderByDescending(x => x.Value))
            Console.WriteLine(item.Value + "\t" + item.Key.Title);
    }

    private static List<Tuple<ValidationResult, string>> ValidateFile(string file, IEnumerable<string> prefixes,
        Dictionary<int, List<IBibItem>> yearItemsDict)
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
        var list = matches.Select(x => Validate2(prefixes, text, x, yearItemsDict)).ToList();
        return list;
    }

    private static Tuple<ValidationResult, string> Validate2(IEnumerable<string> prefixes, string text, Match match,
        Dictionary<int, List<IBibItem>> items)
    {
        var result = Validate(prefixes, text, match, items);
        var range = Utils.SafeRange(match.Index, 50, 4);
        var kvp = new Tuple<ValidationResult, string>(result, text[range]);

        return kvp;
    }

    private static ValidationResult Validate(IEnumerable<string> prefixes, string text, Capture match,
        IDictionary<int, List<IBibItem>> yearItemsDict)
    {
        var ind = match.Index;
        var hasPrefix = prefixes
            .Select(prefix => CompareBackwards(text, ind, prefix))
            .Any(x => x);

        // e.g. im Jahr 2013
        if (hasPrefix)
            return new ValidationResult {ValType = ValidationType.NoRef};

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
        var group = yearItemsDict.TryGetValue(year);
        if (group == null) return new ValidationResult {ValType = ValidationType.NoYear};

        foreach (var item in @group)
        {
            var list = item.Authors.Select(x => x.Key).ToList();
            var authors = list.Count switch
            {
                1 => list[0],
                2 => (list[0] + "/" + list[1]),
                _ => list[0] + " et al."
            };

            var isValid = CompareBackwards(text, ind, authors);
            if (isValid) return new ValidationResult {ValType = ValidationType.Valid, BibItem = item};
        }

        var section = text[Utils.SafeRange(ind, 50)];
        foreach (var item in @group)
        {
            var list = item.Authors.Select(x => x.Key).ToList();
            var isFormat = list.All(section.Contains);
            if (isFormat) return new ValidationResult {ValType = ValidationType.Format, BibItem = item};
        }

        return new ValidationResult {ValType = ValidationType.Invalid};
    }

    private static bool CompareBackwards(string text, int index, string str)
    {
        var range = Utils.SafeRange(index - 1, str.Length);
        return text[range] == str;
    }
}