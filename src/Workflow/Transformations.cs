namespace Workflow;

public static class Transformations
{
    private const string _pandoc2 = @"C:\Program Files\pandoc\2.17.0.1\pandoc.exe";
    private const string _pandoc3 = @"C:\Program Files\pandoc\3.1.3\pandoc.exe";
    private const string _srcPath = @"D:\7_code\phd-private";

    public static void Doc2Md()
    {
        var docxPath = Path.Combine(_srcPath, "docx");
        var files = Directory.EnumerateFiles(docxPath, "*.docx");

        foreach (var file in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var outFile = Path.Combine(_srcPath, "md", fileName + ".md");
            var args = new[] {"--from=docx", "--to=markdown", "--output=" + outFile, file};

            System.Diagnostics.Process.Start(_pandoc3, args);

            Thread.Sleep(2000);
            Console.WriteLine(outFile);
        }
    }

    public static void Md2Tex()
    {
        var mdPath = Path.Combine(_srcPath, "md");
        var files = Directory.EnumerateFiles(mdPath, "*.md.md").ToList();

        foreach (var file in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var outFile = Path.Combine(_srcPath, "tex", fileName + ".tex");
            var args = new[] {"--from=markdown-auto_identifiers", "--to=latex", "--output=" + outFile, file};
            var result = System.Diagnostics.Process.Start(_pandoc3, args);

            Thread.Sleep(5000);
            Console.WriteLine(result);
            Console.WriteLine(outFile);
        }
    }

    public static void Tex2Docx()
    {
        var file = @"D:\7_Code\phd-private\tex\test.md.tex";

        var outFile = Path.Combine(_srcPath, "docout", Path.GetFileName(file) + ".docx");
        var args = new[] {"--from=latex", "--to=docx", "--output=" + outFile, file};

        var result = System.Diagnostics.Process.Start(_pandoc3, args);

        Thread.Sleep(5000);
        Console.WriteLine(result);
        Console.WriteLine(outFile);
    }

    public static void Md2Docx()
    {
        var mdPath = Path.Combine(_srcPath, "md");
        var files = Directory.EnumerateFiles(mdPath, "*.md.md").ToList();

        var file = @"D:\7_Code\phd-private\tex\complete18mai_.tex";

        var fileName = Path.GetFileNameWithoutExtension(file);
        var outFile = Path.Combine(_srcPath, "docout", fileName + ".docx");
        var args = new[] {"--from=latex", "--to=docx", "--output=" + outFile, file};

        var result = System.Diagnostics.Process.Start(_pandoc3, args);

        Thread.Sleep(2000);
        Console.WriteLine(result);
        Console.WriteLine(outFile);
    }
}