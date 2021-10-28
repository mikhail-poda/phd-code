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
            ValidateReferences();
        }

        private static void ValidateReferences()
        {
            var risPath = @"D:\code\Mikhail\phd-private\bibliography\bibliography.ris";
            var items = new RisBibliography(risPath).Items
                .GroupBy(x => x.Year)
                .ToDictionary(
                    x => x.Key,
                    x => x.OrderByDescending(y => y.Authors.Count).ToList());

            var folder = @"D:\code\Mikhail\phd-private\md";
            var files = Directory.EnumerateFiles(folder, "*.md");
            var count = 0;
            Range range;
            var prefixes = new[]
            {
                "Jahr",
                "Jahren",
                "the",
                "der",
                "den",
                "das",
                "Die",
                "erstmals",
                "nach",
                "von",
                "Von",
                "Sommer",
                "wurde",
                "war",
                "was",
                "bis",
                "Bis",
                "erst",
                "Erst",
                "bereits",
                "Bereits",
                "um",
                "und",
                "July",
                "March",
                "Januar",
                "Februar",
                "März",
                "April",
                "Mai",
                "Juni",
                "Juli",
                "August",
                "September",
                "Oktober",
                "November",
                "Dezember",
                "\\"
            };

            foreach (var file in files)
            {
                Console.WriteLine(file);

                var text = File
                    .ReadAllText(file)
                    .Replace('\n', ' ')
                    .Replace('\r', ' ')
                    .Replace('>', ' ')
                    .Replace("     ", " ")
                    .Replace("    ", " ")
                    .Replace("   ", " ")
                    .Replace("  ", " ");

                var matches = Regex.Matches(text, "\\d{4}");
                var len = text.Length;

                foreach (Match match in matches)
                {
                    var isValid = Validate(prefixes, text, match, items);
                    if (isValid) continue;

                    range = new Range(Math.Max(0, match.Index - 50), match.Index + 4);
                    Console.WriteLine(text[range] + "         " + isValid);
                    count++;
                }
            }

            Console.WriteLine(count);
        }

        private static bool Validate(IEnumerable<string> prefixes, string text, Capture match,
            IDictionary<int, List<IBibItem>> items)
        {
            var hasPrefix = prefixes
                .Select(prefix => CompareBackwards(text, match.Index, prefix))
                .Any(x => x);

            if (hasPrefix)
                return true;

            var ind = match.Index;
          if (text[ind - 1] == '(') ind -= 1;

            var year = int.Parse(match.Value);
            var group = items.TryGetValue(year);
            if (group == null) return false;

            foreach (var item in @group)
            {
                var list = item.Authors.Select(x => x.Key).ToList();
                var authors = string.Join(", ", list);
                if (authors.IsNullOrEmpty()) continue;

                var isValid = CompareBackwards(text, ind, authors);
                if (isValid) return true;

                if (list.Count == 2)
                {
                    authors = list[0] + "/" + list[1];
                    isValid = CompareBackwards(text, ind, authors);
                    if (isValid) return true;
                }
                else if (list.Count > 2)
                {
                    authors = list[0] + " et al.";
                    isValid = CompareBackwards(text, ind, authors);
                    if (isValid) return true;
                }
            }

            return false;
        }

        private static bool CompareBackwards(string text, int index, string str)
        {
            var range = new Range(index - 1 - str.Length, index - 1);
            return text[range] == str;
        }

        private static void ValidateBib()
        {
            var risPath = @"D:\code\Mikhail\phd-private\bibliography\bibliography.ris";
            var items = new RisBibliography(risPath).Items;

            Console.WriteLine("   ------------------------ Year -----------------------   ");

            // validate year
            foreach (var item in items)
                if (item.Year is < 1900 or > 2020)
                    Console.WriteLine("No year: " + item.Type + ": " + item.Title);

            var years = items
                .GroupBy(x => x.Year)
                .OrderBy(x => x.Key);

            foreach (var year in years)
                Console.WriteLine(year.Key + ": " + year.Count());

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