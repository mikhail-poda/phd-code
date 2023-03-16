using System.Text;
using Library;

namespace Workflow.Bibliography;

public static class ValidationBib
{
    public static void Validate()
    {
        var risFile = @"D:\7_Code\phd-private\bibliography\bibliography.ris";
        var items = new RisBibliography(risFile).ToList();
        var sb = new StringBuilder();

        Validate(sb, items);

        var outFile = risFile + ".valid.txt";
        File.WriteAllText(outFile, sb.ToString());
        Console.WriteLine(outFile);
    }

    private static void Validate(StringBuilder sb, IList<IBibItem> items)
    {
        sb.AppendLine("   ------------------------ Last Char Point -----------------------   ");
        foreach (var item in items)
        foreach (var field in item)
            if (!field.Value.IsNullOrEmpty() && field.Value.Last() == '.')
                sb.AppendLine("Last char is point: " + item.Type + ": " + item.Title + ": " + field.Value);

        // validate year
        sb.AppendLine("   ------------------------ Year -----------------------   ");

        // validate year
        foreach (var item in items)
            if (item.Year is < 1900 or > 2022)
                sb.AppendLine("No year: " + item.Type + ": " + item.Title);

        var years = items
            .GroupBy(x => x.Year)
            .OrderBy(x => x.Key);

        foreach (var year in years)
            sb.AppendLine(year.Key + ": " + year.Count());

        sb.AppendLine("   ------------------------ Authors -----------------------   ");

        // validate authors
        foreach (var item in items)
            if (item.Authors.IsNullOrEmpty())
                sb.AppendLine("No authors: " + item.Type + ": " + item.Title);

        sb.AppendLine("   ------------------------- Title ----------------------   ");

        // validate title
        foreach (var item in items)
            if (item.Title.IsNullOrEmpty())
                sb.AppendLine("No title: " + item);

        sb.AppendLine("   ------------------------ Pages -----------------------   ");

        // validate pages
        foreach (var item in items)
            if (item.Type is BibType.Article or BibType.Chapter)
                if (item.Pages == null)
                    sb.AppendLine("No pages: " + item.Type + ": " + item.Title);

        sb.AppendLine("   ------------------------ City -----------------------   ");

        // validate city
        var cities = items
            .Select(x => x.PublCity)
            .ToHashSet()
            .OrderBy(x => x);

        foreach (var city in cities)
            sb.AppendLine(city);

        sb.AppendLine("   ------------------------ Publisher -----------------------   ");

        // validate publisher
        foreach (var item in items)
            if (item.Type is BibType.Book)
                if (item.Publisher.IsNullOrEmpty())
                    sb.AppendLine("No Publisher: " + item.Type + ": " + item.Title);

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

        sb.AppendLine("   ----------------------- By Number ------------------------   ");

        foreach (var chunk in chunks.OrderByDescending(x => x.Count))
            sb.AppendLine(chunk.Name + ", " + chunk.Count);

        sb.AppendLine("   ------------------------ By Name -----------------------   ");

        foreach (var chunk in chunks.OrderBy(x => x.Name))
            sb.AppendLine(chunk.Name + ", " + chunk.Count);

        sb.AppendLine("   ------------------------ Cities -----------------------   ");

        foreach (var chunk in chunks.Where(x => x.Cities.Count > 1))
            sb.AppendLine(chunk.Name + ", " + string.Join('/', chunk.Cities));

        sb.AppendLine("   ------------------------ Authors by last name -----------------------   ");
        var authors = items
            .SelectMany(GetAllAuthors)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        foreach (var author in authors)
            sb.AppendLine(author);

        sb.AppendLine("   ------------------------ Authors by first name -----------------------   ");

        foreach (var author in authors.Select(FirstLast).OrderBy(x => x))
            sb.AppendLine(author);

        sb.AppendLine("   ------------------------ Multiple Author-Year -----------------------   ");

        var yearItems = items
            .GroupBy(x => x.Year)
            .OrderBy(x => x.Key)
            .ToList();

        foreach (var year in yearItems)
        foreach (var chunk in year.GroupBy(x => x.Authors[0].Key))
            if (chunk.Count() > 1)
                sb.AppendLine(year.Key + ": " + chunk.Key);

        sb.AppendLine("   ------------------------ Ders. - Dies. -----------------------   ");
        foreach (var item in items)
        {
            var allAuthors = GetAllAuthors(item).ToList();
            if (allAuthors.IsNullOrEmpty()) continue;

            var hasSame = allAuthors.GroupBy(x => x).All(x => x.Count() == 2);
            if (hasSame)
                sb.AppendLine(item.Title + " : " + string.Join("; ", allAuthors));
        }


        sb.AppendLine(items.Count.ToString());
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