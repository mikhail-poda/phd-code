using System.Text;
using System.Text.RegularExpressions;
using Library;

namespace Workflow.Formats;

public static class ValidationMd
{
    public static void PostProcessing()
    {
        const string mdPath = @"D:\7_code\phd-private\md";
        var files = Directory.EnumerateFiles(mdPath, "*.md.md");
        var unprintable = new Dictionary<char, int>();
        var sb = new StringBuilder();

        var refs = new HashSet<string>();
        var labels = new HashSet<string>();

        foreach (var file in files)
        {
            var text = File.ReadAllText(file);

            ValidateSymbols(text, sb);
            CountUnprintable(text, unprintable);

            GatherLabels(text, labels, refs);

            Console.WriteLine(file);
        }

        sb.AppendLine();
        sb.AppendLine("============ All unprintable ============");
        foreach (var @char in unprintable.OrderByDescending(x => x.Value))
            sb.AppendLine(@char.Key + "  " + (int) @char.Key + "  " + @char.Value);

        sb.AppendLine();
        sb.AppendLine($"============ All labels {labels.Count} ============");
        foreach (var str in labels.OrderBy(x => x))
            sb.AppendLine(str);

        sb.AppendLine();
        sb.AppendLine($"============ All refs {refs.Count} ============");
        foreach (var str in refs.OrderBy(x => x))
            sb.AppendLine(str);

        sb.AppendLine();
        sb.AppendLine("============ Missing refs ============");
        foreach (var str in labels.Except(refs).OrderBy(x => x))
            sb.AppendLine(str);

        sb.AppendLine("============ Missing labels ============");
        foreach (var str in refs.Except(labels).OrderBy(x => x))
            sb.AppendLine(str);

        var resFile = Path.Combine(mdPath, "validation.txt");
        Console.WriteLine(resFile);
        File.WriteAllText(resFile, sb.ToString());
    }

    private static void GatherLabels(string text, ISet<string> labels, ISet<string> refs)
    {
        var pattern = @"\\label{(\w+)}";
        var matches = Regex.Matches(text, pattern);

        foreach (Match match in matches)
        {
            var label = match.Groups[1].Value;
            if (labels.Contains(label))
                throw new Exception($"Label {label} already in use");
            labels.Add(label);
        }

        pattern = @"\\ref{(\w+)}";
        matches = Regex.Matches(text, pattern);

        foreach (Match match in matches)
            refs.Add(match.Groups[1].Value);

        pattern = @"\\pageref{(\w+)}";
        matches = Regex.Matches(text, pattern);

        foreach (Match match in matches)
            refs.Add(match.Groups[1].Value);
    }


    private static void ValidateSymbols(string text, StringBuilder sb)
    {
        // website like in <http://www.artecitya.gr/eric-ellingsen.html>
        text = Utils.ToSingleLine(text).Replace("://", "@");

        var patterns = new[]
        {
            "§", // error transforming
            "@", // error transforming references
            "r/i", // Künstler/innen
            "\\df", // (Helguera 2011: 14f.).
            "\\s\\)",
            "\\s\"", // critique'. " (Buchholz/Wuggenig
            "\\s'", // The 'right to the
            "\\(\\s",
            "\\w\"\\w",
            "\\w'\\w",
            "\\s:",
            ":\\S",
            "/\\s",
            "\\s/",
            "\\s\\.",
            "\\s\\,",
            "\\,\\S",
            "\\s\\?",
            "\\s\\!",
            "\\s\\:",
            "\\:\\S"
        };

        foreach (var pattern in patterns)
            ValidatePattern(text, pattern, sb);
    }

    private static void ValidatePattern(string text, string pattern, StringBuilder sb)
    {
        var len = text.Length;
        var matches = Regex.Matches(text, pattern);
        if (matches.Count == 0) return;

        sb.AppendLine();
        sb.AppendLine("--------------- Pattern " + pattern + " " + matches.Count);
        sb.AppendLine();

        foreach (Match match in matches)
        {
            var range = Utils.SafeRange(match.Index, 20, 20, len);
            sb.AppendLine("\t\t" + text[range]);
        }
    }

    private static void CountUnprintable(string text, IDictionary<char, int> hset)
    {
        foreach (var @char in text)
        {
            if (Utils.IsPrintable(@char)) continue;

            if (hset.ContainsKey(@char))
                hset[@char]++;
            else
                hset.Add(@char, 1);
        }
    }
}