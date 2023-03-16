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

        foreach (var file in files)
        {
            var text = File.ReadAllText(file);

            ValidateSymbols(text, sb);
            CountUnprintable(text, unprintable);

            Console.WriteLine(file);
        }

        foreach (var @char in unprintable.OrderByDescending(x => x.Value))
            sb.AppendLine(@char.Key + "  " + (int) @char.Key + "  " + @char.Value);

        var resFile = Path.Combine(mdPath, "validation.txt");
        Console.WriteLine(resFile);
        File.WriteAllText(resFile, sb.ToString());
    }


    private static void ValidateSymbols(string text, StringBuilder sb)
    {
        text = Utils.ToSingleLine(text).Replace("://", "@");

        var patterns = new[]
        {
            "r/i", // Künstler/innen
            "\\df", // (Helguera 2011: 14f.).
            "\\s\\)",
            "\\(\\s",
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
        sb.AppendLine("--------------- Pattern \"" + pattern + "\" " + matches.Count);
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