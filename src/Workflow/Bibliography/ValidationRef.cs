using System.Globalization;
using System.Text;
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
        var risFile = @"D:\7_code\phd-private\bibliography\bibliography.ris";
        var items = new RisBibliography(risFile).ToList();
        var sb = new StringBuilder();

        Validate(sb, items);

        var outFile = risFile + ".ref.txt";
        File.WriteAllText(outFile, sb.ToString());
        Console.WriteLine(outFile);
    }

    public static void Validate(StringBuilder sb, IList<IBibItem> items)
    {
        var yearItemsDict = items
            .Where(x => x.Type != BibType.WebPage)
            .GroupBy(x => x.Year)
            .ToDictionary(
                x => x.Key,
                x => x.OrderByDescending(y => y.Authors.Count).ToList());

        var itemCountDict = items.ToDictionary(x => x, x => 0);

        var folder = @"D:\7_code\phd-private\md";
        var files = Directory.EnumerateFiles(folder, "*.md.md").ToList();

        var prefixes = File
            .ReadAllLines("IgnoreNames.txt")
            .Select(x => x.Length <= 3 ? " " + x : x)
            .ToList();

        var list1 = files
            .Select(f => ValidateByYear(f, prefixes, yearItemsDict, sb))
            .SelectMany(x => x)
            .ToArray();

        var websites = items.Where(x => x.Type == BibType.WebPage).ToList();
        var list2 = files
            .Select(f => ValidateWebsite(f, websites, sb))
            .SelectMany(x => x)
            .ToArray();

        var list = Enumerable.Concat(list1, list2);

        sb.AppendLine("------------------- Valid -------------------");
        foreach (var match in list.Where(x => x.Item1.ValType == ValidationType.Valid))
            sb.AppendLine(match.Item2);

        sb.AppendLine("------------------- NoYear -------------------");
        foreach (var match in list.Where(x => x.Item1.ValType == ValidationType.NoYear))
            sb.AppendLine(match.Item2);

        sb.AppendLine("------------------- NoRef -------------------");
        foreach (var match in list.Where(x => x.Item1.ValType == ValidationType.NoRef))
            sb.AppendLine(match.Item2);

        sb.AppendLine("------------------- Invalid -------------------");
        foreach (var match in list.Where(x => x.Item1.ValType == ValidationType.Invalid))
            sb.AppendLine(match.Item2);

        sb.AppendLine("------------------- Format Issue -------------------");
        foreach (var match in list.Where(x => x.Item1.ValType == ValidationType.Format))
            sb.AppendLine(match.Item2);

        sb.AppendLine("------------------- Use -------------------");
        foreach (var match in list.Where(x => x.Item1.BibItem != null))
            itemCountDict[match.Item1.BibItem] += 1;

        foreach (var item in itemCountDict.OrderByDescending(x => x.Value))
            sb.AppendLine(item.Value + "\t" + item.Key.Title + "\t" + item.Key.Year + "\t" +
                          string.Join(", ", item.Key.Authors.Select(x => x.Key)));
    }

    private static IList<Tuple<ValidationResult, string>> ValidateWebsite(string file, IList<IBibItem> websites,
        StringBuilder sb)
    {
        sb.AppendLine(file);

        var lines = File.ReadAllLines(file).Where(x => !x.StartsWith("> ")).ToList();
        var text = string.Join(' ', lines);
        text = Regex.Replace(text, @"\s+", " ");

        var matches = Regex.Matches(text, "vgl. Website");
        var list = matches.Select(x => ValidateWebsiteDescr(text, x, websites)).ToList();

        return list;
    }

    private static Tuple<ValidationResult, string> ValidateWebsiteDescr(string text, Capture match,
        IList<IBibItem> websites)
    {
        var result = ValidateWebsite(text, match, websites);
        var range = Utils.SafeRange(match.Index, 4, 50);
        var kvp = new Tuple<ValidationResult, string>(result, text[range]);

        return kvp;
    }

    private static ValidationResult ValidateWebsite(string text, Capture match, IList<IBibItem> websites)
    {
        var ind = match.Index;
        foreach (var website in websites)
        {
            var str = $"vgl. Website {website.Title}";
            var isValid = CompareForwards(text, ind, str);

            if (isValid)
                return new ValidationResult {ValType = ValidationType.Valid, BibItem = website};
        }

        return new ValidationResult {ValType = ValidationType.Invalid};
    }


    private static IList<Tuple<ValidationResult, string>> ValidateByYear(string file, IEnumerable<string> prefixes,
        IDictionary<int, List<IBibItem>> yearItemsDict, StringBuilder sb)
    {
        sb.AppendLine(file);

        var lines = File.ReadAllLines(file).Where(x => !x.StartsWith("> ")).ToList();
        var text = string.Join(' ', lines);
        text = Regex.Replace(text, @"\s+", " ");

        var matches = Regex.Matches(text, "\\d{4}");
        var list = matches.Select(x => ValidateByYearDescr(prefixes, text, x, yearItemsDict)).ToList();

        return list;
    }

    private static Tuple<ValidationResult, string> ValidateByYearDescr(IEnumerable<string> prefixes, string text,
        Capture match,
        IDictionary<int, List<IBibItem>> yearItemsDict)
    {
        var result = ValidateByYear(prefixes, text, match, yearItemsDict);
        var range = Utils.SafeRange(match.Index, 50, 4);
        var kvp = new Tuple<ValidationResult, string>(result, text[range]);

        return kvp;
    }

    private static ValidationResult ValidateByYear(IEnumerable<string> prefixes, string text, Capture match,
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
        var txt = text[range];
        return txt == str;
    }

    private static bool CompareForwards(string text, int index, string str)
    {
        var range = Utils.SafeRange(index, 0, str.Length);
        var txt = text[range];
        return txt == str;
    }
}