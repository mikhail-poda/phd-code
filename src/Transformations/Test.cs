using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Library;

namespace PhdCode
{
    static class Program
    {
        static void Main()
        {
            //ValidationRef.Validate();
            ValidationBib.Validate();
            //Md2Tex();
        }

        private static void Md2Tex()
        {
            var pandoc = @"C:\Program Files\Pandoc\pandoc.exe";
            var srcPath = @"D:\code\phd-private";
            var mdPath = Path.Combine(srcPath, "md");
            var files = Directory.EnumerateFiles(mdPath, "*.md").ToList();

            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var outFile = Path.Combine(srcPath, "tex", fileName + ".tex");
                var args = new[] {"--from=markdown", "--to=latex", "--output=" + outFile, file};
                var result = System.Diagnostics.Process.Start(pandoc, args);
                
                Console.WriteLine(result);
            }
        }

        private static void Doc2Md()
        {
            var pandoc = @"C:\Program Files\Pandoc\pandoc.exe";
            var srcPath = @"D:\code\phd-private";
            Scripts.Doc2Md(pandoc, srcPath);
        }
    }
}