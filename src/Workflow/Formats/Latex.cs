using System.Text.RegularExpressions;

namespace Workflow.Formats;

public static class Latex
{
    public static void PostProcessing()
    {
        var texPath = @"D:\code\phd-private\tex";
        var files = Directory.EnumerateFiles(texPath, "*.tex");

        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
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

            if (text.Contains("longtable"))
            {
                var i0 = Regex.Match(text, "\\\\begin\\{longtable\\}");
                var i1 = Regex.Match(text, "\\\\end\\{longtable\\}");
                var sub1 = text[0..i0.Index];
                var sub2 = text[(i1.Index + 16)..];

                text = sub1 +"@PUT TABLE HERE@"+ sub2;
            }

            File.WriteAllText(file, text);
        }
    }
}