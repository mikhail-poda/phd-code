namespace PhdCode;

public static class Transformations
{
    private static string _pandoc = @"C:\Program Files\Pandoc\pandoc.exe";
    private static string _srcPath = @"D:\code\phd-private";
    
    public static void Doc2Md()
    {
        var docxPath = Path.Combine(_srcPath, "docx");
        var files = Directory.EnumerateFiles(docxPath, "*.docx");

        foreach (var file in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var outFile = Path.Combine(_srcPath, "md", fileName + ".md");
            var args = new[] {"--from=docx", "--to=markdown", "--output=" + outFile, file};

            System.Diagnostics.Process.Start(_pandoc, args);

            Thread.Sleep(2000);
        }
    }
    
    public static void Md2Tex()
    {
        var mdPath = Path.Combine(_srcPath, "md");
        var files = Directory.EnumerateFiles(mdPath, "*.md").ToList();

        foreach (var file in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var outFile = Path.Combine(_srcPath, "tex", fileName + ".tex");
            var args = new[] {"--from=markdown-auto_identifiers", "--to=latex", "--output=" + outFile, file};
            var result = System.Diagnostics.Process.Start(_pandoc, args);

            Console.WriteLine(result);
        }
    }
    
    public static void Md2Docx()
    {
        var mdPath = Path.Combine(_srcPath, "md");
        var file = Path.Combine(mdPath, "document.md");
        var outFile = Path.Combine(mdPath, "document.docx");

        var args = new[] {"--from=markdown", "--to=docx", "--output=" + outFile, file};
        var result = System.Diagnostics.Process.Start(_pandoc, args);

        Console.WriteLine(result.ExitCode);
    }
}