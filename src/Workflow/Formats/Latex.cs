﻿using System.Text.RegularExpressions;
using Library;

namespace Workflow.Formats;

public static class Latex
{
    public static void PostProcessing()
    {
        var texPath = @"D:\code\phd-private\tex";
        var files = Directory.EnumerateFiles(texPath, "*.tex");

        foreach (var file in files)
        {
            Console.WriteLine(file);

            var text = File.ReadAllText(file);

            text = ReplaceSpecialChars(text);
            text = RemoveTableText(text);

            ValidateText(text);

            File.WriteAllText(file, text);
        }
    }

    private static void ValidateText(string text)
    {
        var i0 = text.IndexOf(" *");
        if (i0 >= 0) Console.WriteLine(text[Utils.SafeRange(i0, 30, 30, text.Length)]);

        var i1 = text.IndexOf("* ");
        if (i1 >= 0) Console.WriteLine(text[Utils.SafeRange(i1, 30, 30, text.Length)]);
    }

    private static string RemoveTableText(string text)
    {
        if (!text.Contains("longtable")) return text;

        var i0 = Regex.Match(text, "\\\\begin\\{longtable\\}");
        var i1 = Regex.Match(text, "\\\\end\\{longtable\\}");
        var sub1 = text[0..i0.Index];
        var sub2 = text[(i1.Index + 16)..];

        text = sub1 + "@PUT TABLE HERE@" + sub2;

        return text;
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
            .Replace("\r\n  ", "\r\n");
        return text;
    }
}