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
            //ValidationBib.Validate();
            //ValidationRef.Validate();

            //Doc2Md();
            //MdPostProcessing();

            //Md2Tex();
            TexPostProcessing();

            //Md2Docx();
        }

        private static void Doc2Md()
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

        private static void MdPostProcessing()
        {
            var mdPath = @"D:\code\phd-private\md";
            var files = Directory.EnumerateFiles(mdPath, "*.md");
            var unprintable = new Dictionary<char, int>();

            foreach (var file in files)
            {
                var text = File.ReadAllText(file);

                text = RemoveLiterature(text);
                text = CorrectQuotes(text);
                text = ReplaceUnprintable(text);
                text = MakeHeaders(text);

                CountUnprintable(text, unprintable);

                File.WriteAllText(file, text);
            }

            foreach (var @char in unprintable.OrderByDescending(x => x.Value))
                Console.WriteLine(@char.Key + "  " + (int) @char.Key + "  " + @char.Value);
        }

        private static void TexPostProcessing()
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

        private static string CorrectQuotes(string text)
        {
            text = text
                .Replace("*** ***", " ")
                .Replace("** **", " ")
                .Replace("* *", " ")
                .Replace("*,*", ",")
                .Replace("*.*", ",")
                .Replace("***\r\n***", " ")
                .Replace("**\r\n**", " ")
                .Replace("*\r\n*", " ")
                .Replace("„*", "*„")
                .Replace("*\"", "\"*");

            return text;
        }

        private static string ReplaceUnprintable(string text)
        {
            text = text
                .Replace("ä", "ä") // a ̈ -> ä
                .Replace("ö", "ö")
                .Replace("ü", "ü")
                .Replace("Ä", "Ä");
            
            return text;
        }

        private static void CountUnprintable(string text, IDictionary<char, int> hset)
        {
            foreach (var @char in text)
            {
                if (IsPrintable(@char)) continue;

                if (hset.ContainsKey(@char))
                    hset[@char]++;
                else
                    hset.Add(@char, 1);
            }
        }

        private static bool IsPrintable(char @char)
        {
            if (@char == '\r') return true;
            if (@char == '\n') return true;

            if (@char < ' ') return false;
            if (@char <= '~') return true;

            if (@char == 'ü') return true;
            if (@char == 'ö') return true;
            if (@char == 'ä') return true;
            if (@char == 'ß') return true;
            if (@char == 'Ä') return true;
            if (@char == 'Ö') return true;
            if (@char == 'Ü') return true;

            return false;
        }

        private static string MakeHeaders(string text)
        {
            var chapters = Regex.Matches(text, "\\*\\*\\d\\.\\s(.|\n|\r)+?\\*\\*");
            foreach (Match match in chapters)
            {
                var name = match.Value[5..^2].Replace("\r\n", " ");
                text = text.Replace(match.Value, "# " + name);
            }

            var sections = Regex.Matches(text, "\\*\\*\\d\\.\\d\\s(.|\n|\r)+?\\*\\*");
            foreach (Match match in sections)
            {
                var name = match.Value[6..^2].Replace("\r\n", " ");
                text = text.Replace(match.Value, "## " + name);
            }

            var subsec = Regex.Matches(text, "\\*\\*\\d\\.\\d\\.\\d\\s(.|\n|\r)+?\\*\\*");
            foreach (Match match in subsec)
            {
                var name = match.Value[8..^2].Replace("\r\n", " ");
                text = text.Replace(match.Value, "### " + name);
            }

            var subsub = Regex.Matches(text, "\\*\\*\\d\\.\\d\\.\\d\\.\\d\\s(.|\n|\r)+?\\*\\*");
            foreach (Match match in subsub)
            {
                var name = match.Value[10..^2].Replace("\r\n", " ");
                text = text.Replace(match.Value, "#### " + name);
            }

            return text;
        }

        private static string RemoveLiterature(string text)
        {
            var il = text.IndexOf("**Literatur");
            if (il == -1) il = text.IndexOf("**[Literatur");
            if (il == -1) il = text.IndexOf("[Literatur");
            if (il == -1) return text;

            var ir = text.IndexOf("[^1]:");

            text = text.Substring(0, il) + text.Substring(ir);
            return text;
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
                var args = new[] {"--from=markdown-auto_identifiers", "--to=latex", "--output=" + outFile, file};
                var result = System.Diagnostics.Process.Start(pandoc, args);

                Console.WriteLine(result);
            }
        }

        private static void Md2Docx()
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
}