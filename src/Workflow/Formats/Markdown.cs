using System.Text;
using System.Text.RegularExpressions;

namespace Workflow.Formats;

public static class Markdown
{
    public static void PostProcessing()
    {
        const string mdPath = @"D:\7_code\phd-private\md";
        var files = Directory.EnumerateFiles(mdPath, "*.md");

        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            text = PostProcessing(text);

            var resFile = file + ".md";
            File.WriteAllText(resFile, text);
            Console.WriteLine(resFile);
        }
    }

    private static string PostProcessing(string text)
    {
        text = CutTablesImagesToc(text);

        text = CorrectQuotes(text);
        text = CorrectQuotes(text);

        text = ReplaceUnprintable(text);
        text = MakeHeaders(text);

        text = CorrectQuotations(text);

        return text;
    }

    private static string CorrectQuotations(string text)
    {
        var sb = new StringBuilder();
        var cell = text.Split("\r\n");

        var isQuotation = false;

        foreach (var lineIn in cell)
        {
            var line = lineIn.TrimEnd();

            if (line.StartsWith(@"> \$"))
            {
                if (isQuotation) throw new Exception("Already in quotation");
                isQuotation = true;
                line = line.Replace(@"\$", null);
            }
            else if (line.StartsWith(@"\$"))
            {
                if (isQuotation) throw new Exception("Already in quotation");
                isQuotation = true;
                line = "> " + line.Replace(@"\$", null);;
            }
            else if (line.EndsWith(@"\$\$"))
            {
                if (!isQuotation) throw new Exception("Should be in quotation");
                isQuotation = false;
                line = line.Replace(@"\$\$", null);
                if (!line.StartsWith('>')) line = "> " + line;
            }
            else if (isQuotation)
            {
                if (!line.StartsWith('>')) line = "> " + line;
            }

            sb.AppendLine(line);
        }

        return sb.ToString();
    }

    private static string CutTablesImagesToc(string text)
    {
        var toc = @"\\@toc_start(.|\n)*\\@toc_end";
        var img = @"\\@image_start(.|\n)*\\@image_end";
        var tbl = @"\\@table_start(.|\n)*\\@table_end";
        var cell = new[] {toc, img, tbl};

        foreach (var pattern in cell)
            text = Regex.Replace(text, pattern, "");

        return text;
    }

    private static string CorrectQuotes(string text)
    {
        text = text
            .Replace("* *", " ")
            .Replace("*,*", ",")
            .Replace("*.*", ".")
            .Replace("*\r\n*", " ")
            .Replace("„*", "*„")
            .Replace("*\"", "\"*")
            .Replace("*§", "§*");

        return text;
    }

    private static string ReplaceUnprintable(string text)
    {
        text = text
            .Replace("ä", "ä") // a ̈ -> ä
            .Replace("ö", "ö")
            .Replace("ü", "ü")
            .Replace("Ä", "Ä")
            .Replace("Ä", "Ä")
            .Replace(" ", " ");

        return text;
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
}