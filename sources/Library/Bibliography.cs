// See https://en.wikipedia.org/wiki/RIS_(file_format) for more information

namespace Library;

using Kvp = KeyValuePair<string, string>;

public enum BibType
{
    Book,
    Chapter,
    WebPage,
    Article,
    Thesis,
}

public interface IBibItem
{
    public int Year { get; }
    public BibType Type { get; }
    public string Title { get; }
    public IList<Kvp> Authors { get; }
}

public interface IBibliography
{
    public IList<IBibItem> Items { get; }
}

public class RisBibliography : IBibliography
{
    public IList<IBibItem> Items { get; }

    public RisBibliography(string fileName)
    {
        if (!fileName.ToLower().EndsWith(".ris") || !File.Exists(fileName))
            throw new Exception("Must be a path to a valid RIS file: " + fileName);

        var text = File.ReadAllLines(fileName);
        var items = GetRisItems(text).Cast<IBibItem>().ToList();

        Items = items;
    }

    private static IEnumerable<RisItem> GetRisItems(IEnumerable<string> text)
    {
        RisItem item = null;

        foreach (var line in text)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("ER"))
            {
                if (item != null)
                {
                    yield return item;
                    item = null;
                }

                continue;
            }

            var kvp = GetRisKvp(line);
            if (kvp == null) continue;

            if (item == null) item = new RisItem();
            item.Add(kvp.Value);
        }

        if (item != null) yield return item;
    }

    private static Kvp? GetRisKvp(string line)
    {
        var ind = line.IndexOf('-');
        if (ind < 1) return null;

        var key = line[..(ind - 1)].Trim();
        var value = line[(ind + 1)..].Trim();

        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) return null;
        return new Kvp(key, value);
    }
}

public record RisItem : IBibItem
{
    private readonly IList<Kvp> _fields = new List<Kvp>();

    public int Year { get; private set; }
    public string Title { get; private set; }
    public BibType Type { get; private set; }

    private IList<Kvp>? _authors;

    public IList<Kvp> Authors
    {
        get { return _authors ?? (_authors = GetAuthors()); }
    }

    private IList<Kvp> GetAuthors()
    {
        var list =
            _fields.Where(x => x.Key == "AU").NullIfEmpty() ??
            _fields.Where(x => x.Key == "A1").NullIfEmpty() ??
            _fields.Where(x => x.Key == "A2").NullIfEmpty() ??
            _fields.Where(x => x.Key == "A3").NullIfEmpty();

        return list.Select(x => GetNameKvp(x.Value)).ToList();
    }

    private static Kvp GetNameKvp(string str)
    {
        var pair = str.Split(',');
        return pair.Length == 1 ? new Kvp(str, null) : new Kvp(pair[0].Trim(), pair[1].Trim());
    }

    public void Add(Kvp kvp)
    {
        switch (kvp.Key)
        {
            case "PY":
                Year = int.Parse(kvp.Value);
                break;
            case "TY":
                Type = GetItemType(kvp.Value);
                break;
            case "TI":
                Title = kvp.Value;
                break;
            default:
                _fields.Add(kvp);
                break;
        }
    }

    private static BibType GetItemType(string kvpValue)
    {
        switch (kvpValue)
        {
            case "CHAP": return BibType.Chapter;
            case "BOOK": return BibType.Book;
            case "JOUR": return BibType.Article;
            case "THES": return BibType.Thesis;
            case "ELEC": return BibType.WebPage;
            default: throw new Exception("Unknown bib type: " + kvpValue);
        }
    }
}