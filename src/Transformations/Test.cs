using System;
using System.IO;
using System.Text;
using Library;

namespace PhdCode
{
    static class Program
    {
        static void Main()
        {
            TestRis();
        }

        private static void TestRis()
        {
            var risPath = @"D:\code\Mikhail\phd\Sources\bibliography\bibliography.ris";
            var items = new RisBibliography(risPath).Items;

            foreach (var item in items)
            {
                if (item.Year < 1900 || item.Year > 2020)
                    Console.WriteLine(item.Type + ": " + item.Title);
            }

            Console.WriteLine(items.Count);
            Console.ReadLine();
        }

        private static void Doc2Md()
        {
            var pandoc = @"C:\Program Files\Pandoc\pandoc.exe";
            var srcPath = @"D:\code\Mikhail\phd\Sources";
            Scripts.Doc2Md(pandoc, srcPath);
        }
    }
}