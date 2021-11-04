namespace PhdCode;

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

            File.WriteAllText(file, text);
        }
    }
}