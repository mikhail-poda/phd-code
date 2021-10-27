namespace Library;

public static class Scripts
{
    public static void Doc2Md(string pandoc, string srcPath)
    {
        var docxPath = Path.Combine(srcPath, "docx");
        var files = Directory.EnumerateFiles(docxPath, "*.docx");

        foreach (var file in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var outFile = Path.Combine(srcPath, "md", fileName + ".md");
            var args = new[] {"--from=docx", "--to=markdown", "--output=" + outFile, file};
            System.Diagnostics.Process.Start(pandoc, args);
        }
    }
}