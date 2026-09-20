using System.Text;
using System.Xml;

var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var tmp = Path.Combine(root, "tools", "tmp");
var dict = Path.Combine(root, "dict");
Directory.CreateDirectory(dict);

Convert(Path.Combine(tmp, "eng-rus.tei"), Path.Combine(dict, "en-ru.tsv"));
Convert(Path.Combine(tmp, "rus-eng.tei"), Path.Combine(dict, "ru-en.tsv"));

static void Convert(string teiPath, string tsvPath)
{
    var map = new Dictionary<string, List<string>>(StringComparer.Ordinal);
    using var reader = XmlReader.Create(teiPath, new XmlReaderSettings
    {
        DtdProcessing = DtdProcessing.Ignore,
        IgnoreComments = true,
        IgnoreWhitespace = true
    });

    string? orth = null;
    var quotes = new List<string>();
    var inEntry = false;
    var inOrth = false;
    var inQuote = false;
    var inTransCit = false;
    var buffer = new StringBuilder();

    while (reader.Read())
    {
        if (reader.NodeType == XmlNodeType.Element)
        {
            var local = reader.LocalName;
            if (local == "entry")
            {
                inEntry = true;
                orth = null;
                quotes.Clear();
            }
            else if (inEntry && local == "orth" && orth is null)
            {
                inOrth = true;
                buffer.Clear();
            }
            else if (inEntry && local == "cit")
            {
                inTransCit = reader.GetAttribute("type") == "trans";
            }
            else if (inTransCit && local == "quote")
            {
                inQuote = true;
                buffer.Clear();
            }
        }
        else if (reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.CDATA || reader.NodeType == XmlNodeType.Whitespace)
        {
            if (inOrth || inQuote)
            {
                buffer.Append(reader.Value);
            }
        }
        else if (reader.NodeType == XmlNodeType.EndElement)
        {
            var local = reader.LocalName;
            if (local == "orth" && inOrth)
            {
                orth = NormalizeHeadword(buffer.ToString());
                inOrth = false;
            }
            else if (local == "quote" && inQuote)
            {
                var quote = CleanGloss(buffer.ToString());
                if (quote.Length > 0 && quotes.Count < 8 && !quotes.Contains(quote, StringComparer.OrdinalIgnoreCase))
                {
                    quotes.Add(quote);
                }

                inQuote = false;
            }
            else if (local == "cit")
            {
                inTransCit = false;
            }
            else if (local == "entry")
            {
                if (!string.IsNullOrEmpty(orth) && quotes.Count > 0)
                {
                    if (!map.TryGetValue(orth, out var list))
                    {
                        list = new List<string>();
                        map[orth] = list;
                    }

                    foreach (var q in quotes)
                    {
                        if (list.Count >= 8)
                        {
                            break;
                        }

                        if (!list.Contains(q, StringComparer.OrdinalIgnoreCase))
                        {
                            list.Add(q);
                        }
                    }
                }

                inEntry = false;
            }
        }
    }

    using var writer = new StreamWriter(tsvPath, false, new UTF8Encoding(false));
    writer.WriteLine("# Source: FreeDict+WikDict (Wiktionary / DBnary). License: CC BY-SA 3.0.");
    foreach (var pair in map.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
    {
        writer.Write(pair.Key);
        writer.Write('\t');
        writer.WriteLine(string.Join(" | ", pair.Value));
    }

    Console.WriteLine($"{Path.GetFileName(tsvPath)}: {map.Count} entries");
}

static string NormalizeHeadword(string value)
{
    var trimmed = value.Trim().ToLowerInvariant().Replace('ё', 'е').Replace('\u0301', '\0');
    return trimmed.Replace("\0", string.Empty);
}

static string CleanGloss(string value)
{
    return value.Trim().Replace("\u0301", string.Empty).Replace('\t', ' ').Replace('\n', ' ');
}
