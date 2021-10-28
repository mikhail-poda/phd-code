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
    public Range? Pages { get; }
    public BibType Type { get; }
    public string Title { get; }
    public string PublCity { get; }
    public string Publisher { get; }
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
    public string PublCity { get; private set; }
    public string Publisher { get; private set; }

    public Range? Pages
    {
        get
        {
            var min = _fields.FirstOrDefault(x => x.Key == "SP");
            var max = _fields.FirstOrDefault(x => x.Key == "EP");

            if (min.Value.IsNullOrEmpty() && max.Value.IsNullOrEmpty()) return null;
            return new Range(int.Parse(min.Value), int.Parse(max.Value));
        }
    }

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

        return list == null ? new List<Kvp> {new Kvp(Title, null)} : list.Select(x => GetNameKvp(x.Value)).ToList();
    }

    private static Kvp GetNameKvp(string str)
    {
        var pair = str.Split(',');
        return pair.Length == 1 ? new Kvp(str, null) : new Kvp(pair[0].Trim(), pair[1].Trim());
    }

    public override string ToString()
    {
        var cell = _fields.Select(x => x.ToString()).ToList();
        return string.Join(' ', cell);
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
            case "CY":
                PublCity = kvp.Value;
                break;
            case "PB":
                Publisher = kvp.Value;
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