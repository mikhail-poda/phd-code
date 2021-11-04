using System.Text.RegularExpressions;
using Library;

namespace PhdCode;

public static class Markdown
{
    public static void PostProcessing()
        {
            var mdPath = @"D:\code\phd-private\md";
            var files = Directory.EnumerateFiles(mdPath, "*.md");
            var unprintable = new Dictionary<char, int>();

            foreach (var file in files)
            {
                Console.WriteLine(file);
                var text = File.ReadAllText(file);

                text = RemoveLiterature(text);
                text = CorrectQuotes(text);
                text = ReplaceUnprintable(text);
                text = MakeHeaders(text);

                ValidateSymbols(text);
                CountUnprintable(text, unprintable);

                File.WriteAllText(file, text);
            }

            foreach (var @char in unprintable.OrderByDescending(x => x.Value))
                Console.WriteLine(@char.Key + "  " + (int) @char.Key + "  " + @char.Value);
        }

        private static void ValidateSymbols(string text)
        {
            ValidatePattern(text, "\\s:");
            ValidatePattern(text, ":\\S");
            ValidatePattern(text, "/\\s");
            ValidatePattern(text, "\\s/");
            ValidatePattern(text, "\\s\\.");
            ValidatePattern(text, "\\.\\S");
            ValidatePattern(text, "\\s\\,");
            ValidatePattern(text, "\\,\\S");
            ValidatePattern(text, "\\s\\?");
            ValidatePattern(text, "\\?\\S");
            ValidatePattern(text, "\\s\\!");
            ValidatePattern(text, "\\!\\S");
            ValidatePattern(text, "\\s\\:");
            ValidatePattern(text, "\\:\\S");
        }

        private static void ValidatePattern(string text, string pattern)
        {
            var len = text.Length;
            var matches = Regex.Matches(text, pattern);
            if (matches.Count == 0) return;
            Console.WriteLine("Pattern \"" + pattern + "\" " + matches.Count);

            foreach (Match match in matches)
            {
                var range = Utils.SafeRange(match.Index, 20, 20, len);
                Console.WriteLine("\t\t" + text[range]);
            }
        }

        private static string CorrectQuotes(string text)
        {
            text = text
                .Replace("*** ***", " ")
                .Replace("** **", " ")
                .Replace("* *", " ")
                .Replace("*,*", ",")
                .Replace("*.*", ",")
                .Replace("***\r\n***", " ")
                .Replace("**\r\n**", " ")
                .Replace("*\r\n*", " ")
                .Replace("„*", "*„")
                .Replace("*\"", "\"*");

            return text;
        }

        private static string ReplaceUnprintable(string text)
        {
            text = text
                .Replace("ä", "ä") // a ̈ -> ä
                .Replace("ö", "ö")
                .Replace("ü", "ü")
                .Replace("Ä", "Ä");

            return text;
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

        private static string MakeHeaders(string text)
        {
            var chapters = Regex.Matches(text, "\\*\\*\\d\\.\\s(.|\n|\r)+?\\*\\*");
            foreach (Match match in chapters)
            {
                var name = match.Value[5..^2].Replace("\r\n", " ");
                text = text.Replace(match.Value, "# " + name);
            }

            var sections = Regex.Matches(text, "\\*\\*\\d\\.\\d\\s(.|\n|\r)+?\\*\\*");
            foreach (Match match in sections)
            {
                var name = match.Value[6..^2].Replace("\r\n", " ");
                text = text.Replace(match.Value, "## " + name);
            }

            var subsec = Regex.Matches(text, "\\*\\*\\d\\.\\d\\.\\d\\s(.|\n|\r)+?\\*\\*");
            foreach (Match match in subsec)
            {
                var name = match.Value[8..^2].Replace("\r\n", " ");
                text = text.Replace(match.Value, "### " + name);
            }

            var subsub = Regex.Matches(text, "\\*\\*\\d\\.\\d\\.\\d\\.\\d\\s(.|\n|\r)+?\\*\\*");
            foreach (Match match in subsub)
            {
                var name = match.Value[10..^2].Replace("\r\n", " ");
                text = text.Replace(match.Value, "#### " + name);
            }

            return text;
        }

        private static string RemoveLiterature(string text)
        {
            var il = text.IndexOf("**Literatur");
            if (il == -1) il = text.IndexOf("**[Literatur");
            if (il == -1) il = text.IndexOf("[Literatur");
            if (il == -1) return text;

            var ir = text.IndexOf("[^1]:");

            text = text.Substring(0, il) + text.Substring(ir);
            return text;
        }
    
}