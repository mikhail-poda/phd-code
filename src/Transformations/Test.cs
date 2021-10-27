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
            ValidateBib();
        }

        private static void ValidateBib()
        {
            var risPath = @"D:\code\Mikhail\phd-private\bibliography\bibliography.ris";
            var items = new RisBibliography(risPath).Items;

            Console.WriteLine("   ------------------------ Year -----------------------   ");

            // validate year
            foreach (var item in items)
                if (item.Year < 1900 || item.Year > 2020)
                    Console.WriteLine("No year: " + item.Type + ": " + item.Title);

            Console.WriteLine("   ------------------------ Authors -----------------------   ");

            // validate authors
            foreach (var item in items)
                if (item.Authors.IsNullOrEmpty())
                    Console.WriteLine("No authors: " + item.Type + ": " + item.Title);

            Console.WriteLine("   ------------------------- Title ----------------------   ");

            // validate title
            foreach (var item in items)
                if (item.Title.IsNullOrEmpty())
                    Console.WriteLine("No title: " + item);

            Console.WriteLine("   ------------------------ Pages -----------------------   ");

            // validate pages
            foreach (var item in items)
                if (item.Type is BibType.Article or BibType.Chapter)
                    if (item.Pages == null)
                        Console.WriteLine("No pages: " + item.Type + ": " + item.Title);

            Console.WriteLine("   ------------------------ City -----------------------   ");

            // validate city
            var cities = items
                .Select(x => x.PublCity)
                .ToHashSet()
                .OrderBy(x => x);

            foreach (var city in cities)
                Console.WriteLine(city);

            Console.WriteLine("   ------------------------ Publisher -----------------------   ");

            // validate publisher
            foreach (var item in items)
                if (item.Type is BibType.Book)
                    if (item.Publisher.IsNullOrEmpty())
                        Console.WriteLine("No Publisher: " + item.Type + ": " + item.Title);

            // extract publishers
            var chunks = items
                .GroupBy(x => x.Publisher)
                .Select(x => new
                {
                    Name = x.Key,
                    Count = x.Count(),
                    Cities = x.Select(y => y.PublCity).ToHashSet()
                })
                .ToList();

            Console.WriteLine("   ----------------------- By Number ------------------------   ");

            foreach (var chunk in chunks.OrderByDescending(x => x.Count))
                Console.WriteLine(chunk.Name + ", " + chunk.Count);

            Console.WriteLine("   ------------------------ By Name -----------------------   ");

            foreach (var chunk in chunks.OrderBy(x => x.Name))
                Console.WriteLine(chunk.Name + ", " + chunk.Count);

            Console.WriteLine("   ------------------------ Cities -----------------------   ");

            foreach (var chunk in chunks.Where(x => x.Cities.Count > 1))
                Console.WriteLine(chunk.Name + ", " + string.Join('/', chunk.Cities));

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