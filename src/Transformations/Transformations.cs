namespace PhdCode;

public static class Transformations
{
    public static void Doc2Md()
    {
        var pandoc = @"C:\Program Files\Pandoc\pandoc.exe";
        var srcPath = @"D:\code\phd-private";

        var docxPath = Path.Combine(srcPath, "docx");
        var files = Directory.EnumerateFiles(docxPath, "*.docx");

        foreach (var file in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var outFile = Path.Combine(srcPath, "md", fileName + ".md");
            var args = new[] {"--from=docx", "--to=markdown", "--output=" + outFile, file};

            System.Diagnostics.Process.Start(pandoc, args);

            Thread.Sleep(2000);
        }
    }
    
    public static void Md2Tex()
    {
        var pandoc = @"C:\Program Files\Pandoc\pandoc.exe";
        var srcPath = @"D:\code\phd-private";
        var mdPath = Path.Combine(srcPath, "md");
        var files = Directory.EnumerateFiles(mdPath, "*.md").ToList();

        foreach (var file in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var outFile = Path.Combine(srcPath, "tex", fileName + ".tex");
            var args = new[] {"--from=markdown-auto_identifiers", "--to=latex", "--output=" + outFile, file};
            var result = System.Diagnostics.Process.Start(pandoc, args);

            Console.WriteLine(result);
        }
    }
    
    public static void Md2Docx()
    {
        var pandoc = @"C:\Program Files\Pandoc\pandoc.exe";
        var srcPath = @"D:\code\phd-private\md";

        var file = Path.Combine(srcPath, "document.md");
        var outFile = Path.Combine(srcPath, "document.docx");

        var args = new[] {"--from=markdown", "--to=docx", "--output=" + outFile, file};
        var result = System.Diagnostics.Process.Start(pandoc, args);

        Console.WriteLine(result.ExitCode);
    }
}