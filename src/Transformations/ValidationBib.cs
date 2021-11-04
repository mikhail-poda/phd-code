using Library;
using Kvp = System.Collections.Generic.KeyValuePair<string, string>;

namespace PhdCode;

public static class ValidationBib
{
    public static void Validate()
    {
        var risPath = @"D:\code\phd-private\bibliography\bibliography.ris";
        var items = new RisBibliography(risPath).ToList();

        Console.WriteLine("   ------------------------ Last Char Point -----------------------   ");
        foreach (var item in items)
            foreach (var field in item)
                if (!field.Value.IsNullOrEmpty() && field.Value.Last() == '.')
                    Console.WriteLine("Last char is point: " + item.Type + ": " + item.Title + ": " + field.Value);

        // validate year
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

        Console.WriteLine("   ------------------------ Authors by last name -----------------------   ");
        var authors = items
            .SelectMany(GetAllAuthors)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        foreach (var author in authors)
            Console.WriteLine(author);

        Console.WriteLine("   ------------------------ Authors by first name -----------------------   ");

        foreach (var author in authors.Select(FirstLast).OrderBy(x => x))
            Console.WriteLine(author);

        Console.WriteLine("   ------------------------ Multiple Author-Year -----------------------   ");

        var yearItems = items
            .GroupBy(x => x.Year)
            .OrderBy(x => x.Key)
            .ToList();

        foreach (var year in yearItems)
            foreach (var chunk in year.GroupBy(x => x.Authors[0].Key))
                if (chunk.Count() > 1)
                    Console.WriteLine(year.Key + ": " + chunk.Key);

        Console.WriteLine(items.Count);
    }

    private static string FirstLast(string author)
    {
        var parts = author.Split(',');
        return parts.Length == 1 ? author : parts[1] + ", " + parts[0];
    }

    private static IList<string> GetAllAuthors(IBibItem bibItem)
    {
        var authors = bibItem
            .Where(x => x.Key.StartsWith('A'))
            .Select(x => x.Value)
            .ToList();

        return authors;
    }
}