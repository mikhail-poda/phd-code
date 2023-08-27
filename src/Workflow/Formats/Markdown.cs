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
        if (text.Contains("{.mark}"))
            throw new Exception("Remove all {.mark}");
        
        text = CutTablesImagesToc(text);

        text = CorrectQuotes(text);
        text = CorrectQuotes(text);

        text = ReplaceUnprintable(text);
        text = MakeTextReference(text);
        text = CorrectQuotations(text);
        text = RemoveSectionNumbers(text);

        return text;
    }

    private static string MakeTextReference(string text)
    {
        // tex \label
        var pattern = @"#(\w+)";
        var replacement = @"\label{$1}";
        var rgx = new Regex(pattern);
        text = rgx.Replace(text, replacement);
        
        // tex \pageref
        pattern = @"S\. \\@(\w+)"; 
        replacement = @"S. \pageref{$1}"; 
        rgx = new Regex(pattern);
        text = rgx.Replace(text, replacement);

        // tex \titleref
        pattern = @"\\@(\w+)"; 
        replacement = @"\titleref{$1}"; 
        rgx = new Regex(pattern);
        text = rgx.Replace(text, replacement);

        return text;
    }

    private static string RemoveSectionNumbers(string text)
    {
        var sb = new StringBuilder();
        var cell = text.Split("\r\n");

        foreach (var lineIn in cell)
        {
            if (!lineIn.StartsWith('#'))
            {
                sb.AppendLine(lineIn);
                continue;
            }

            var parts = lineIn.Split(' ').ToList();
            if (parts[0].Any(x => x != '#'))
                throw new Exception("Bad formatted header: " + lineIn);

            parts.RemoveAt(1);
            sb.AppendLine(string.Join(' ', parts));
        }

        return sb.ToString();
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
                if (isQuotation) throw new Exception("Already in quotation: " + line);
                isQuotation = true;
                line = line.Replace(@"\$", null);
            }
            else if (line.StartsWith(@"\$"))
            {
                if (isQuotation) throw new Exception("Already in quotation: " + line);
                isQuotation = true;
                line = "> " + line.Replace(@"\$", null);
                ;
            }
            else if (line.EndsWith(@"\$\$"))
            {
                if (!isQuotation) throw new Exception("Should be in quotation: " + line);
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
        var toc = @"\\@toc_start(.|\n)*?\\@toc_end";
        var img = @"\\@image_start(.|\n)*?\\@image_end";
        var tbl = @"\\@table_start(.|\n)*?\\@table_end";
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
            .Replace(" ", " ")
            .Replace("⁠", " ");

        return text;
    }
}