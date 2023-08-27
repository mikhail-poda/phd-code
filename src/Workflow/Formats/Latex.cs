using System.Text.RegularExpressions;
using Library;

namespace Workflow.Formats;

public static class Latex
{
    public static void PostProcessing()
    {
        var texPath = @"D:\7_code\phd-private\tex";
        var files = Directory.EnumerateFiles(texPath, "*.tex");

        foreach (var file in files)
        {
            Console.WriteLine(file);

            var text = File.ReadAllText(file);

            text = ReplaceSpecialChars(text);

            ValidateText(text);

            File.WriteAllText(file, text);
        }
    }

    private static void ValidateText(string text)
    {
        var length = text.Length;

        var i0 = text.IndexOf(" *");
        if (i0 >= 0) Console.WriteLine(text[Utils.SafeRange(i0, 30, 30, length)]);

        var i1 = text.IndexOf("* ");
        if (i1 >= 0) Console.WriteLine(text[Utils.SafeRange(i1, 30, 30, length)]);

        var matches = Regex.Matches(text, "href");
        foreach (Match match in matches)
            Console.WriteLine(text[Utils.SafeRange(match.Index, 5, 50, length)]);
    }

    private static string ReplaceSpecialChars(string text)
    {
        text = text
            .Replace("``", "''")
            .Replace("„", "\\glqq ")
            .Replace("'' ", "\\grqq{} ")
            .Replace("''\r\n", "\\grqq{} ")
            .Replace("''", "\\grqq ")
            .Replace("‚", "\\glq ")
            .Replace("' ", "\\grq{} ")
            .Replace("'\r\n", "\\grq{} ")
            .Replace("'", "\\grq ")
            .Replace("=\\textgreater{}", "$\\Rightarrow$")
            .Replace("\\uline", "\\underline")
            .Replace("\r\n  ", "\r\n")
            .Replace("§", "\r\n\\medskip\r\n");

        return text;
    }
}