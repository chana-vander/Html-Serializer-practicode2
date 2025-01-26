using Html_Serializer;
using System.Text.RegularExpressions;
//מהאתר מכבי html שליפת 
//var html = await Load("https://www.maccabi4u.co.il");
var html = await Load("https://chat.malkabruk.co.il/");
var cleanHtml = new Regex("\\s").Replace(html, "");
var htmlLines = new Regex("<(.*?)>").Split(cleanHtml).Where(s => s.Length > 0);
static async Task<string> Load(string url)
{
    HttpClient client = new HttpClient();
    var response = await client.GetAsync(url);
    var html = await response.Content.ReadAsStringAsync();
    return html;
}
//build html tree
HtmlElement root=HtmlElement.BuildTree(htmlLines);
//הצגת העץ

static void PrintTree(HtmlElement element, int level)
{
    var indent = new string(' ', level * 2); // רווחים לכל רמה בעץ
    Console.WriteLine($"{indent}{element.Name} (ID: {element.Id})");

    // הדפסת תוכן פנימי אם יש
    if (!string.IsNullOrEmpty(element.InnerHtml))
    {
        Console.WriteLine($"{indent}  InnerHtml: {element.InnerHtml}");
    }

    // הדפסת Attributes אם יש
    if (element.Attributes.Any())
    {
        Console.WriteLine($"{indent}  Attributes: {string.Join(", ", element.Attributes)}");
    }

    // הדפסת Classes אם יש
    if (element.Classes.Any())
    {
        Console.WriteLine($"{indent}  Classes: {string.Join(", ", element.Classes)}");
    }

    // הדפסת הילדים
    foreach (var child in element.Children)
    {
        PrintTree(child, level + 1);
    }

    // הדפסת תגית סגירה אם יש אלמנטים ילדים
    if (element.Children.Any())
    {
        Console.WriteLine($"{indent}</{element.Name}>");
    }
}


//תגיות ללא סגירה
static string? IsLeaf(string x)
{
    return Array.Find(HtmlHelper.Instance.WithoutClose, element => x.StartsWith(element));
}
static string? IsTag(string x)
{
    string? res = Array.Find(HtmlHelper.Instance.WithoutClose, element => x.StartsWith(element));
    if (res != null)
        return res;
    return Array.Find(HtmlHelper.Instance.Alltags, element => x.StartsWith(element));
}
//מממ מעודכן
/*var html = Load("https://www.matara.pro/nedarimplus/").Result;
var HtmlElement = Serialize(html);*/
//Build tree:
static HtmlElement Serialize(string html)
{
    // Cleaning HTML - removing comments and unnecessary spaces
    var cleanHtml = Regex.Replace(html, @"<!--[\s\S]*?-->", ""); // Removes comments, including multiline comments
    cleanHtml = Regex.Replace(cleanHtml, @"\s*\n\s*|\s{2,}", " "); // Replace multiple spaces with a single space

    // Splitting the HTML into lines as a list of tags
    var htmlLines = new Regex("<(.*?)>").Split(cleanHtml).Where(x => x.Length > 0);

    HtmlElement rootElement = null;
    HtmlElement currentElement = null;

    foreach (var line in htmlLines)
    {
        var firstWord = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        if (string.IsNullOrEmpty(firstWord)) continue;

        if (firstWord.StartsWith("/") && HtmlHelper.Instance.Alltags.Contains(firstWord.Substring(1)))
        {
            // Closing tag - return to the parent element
            if (currentElement != null)
            {
                currentElement = currentElement.Parent;
            }
            continue;
        }

        if (HtmlHelper.Instance.Alltags.Contains(firstWord, StringComparer.OrdinalIgnoreCase) || HtmlHelper.Instance.WithoutClose.Contains(firstWord))
        {
            // Create a new element
            var tagName = firstWord.TrimEnd('/');
            var isSelfClosing = firstWord.EndsWith("/") || HtmlHelper.Instance.WithoutClose.Contains(tagName);

            var newElement = new HtmlElement
            {
                Name = tagName,
                Parent = currentElement
            };

            // Parse attributes and set them
            var attributesRegex = new Regex(@"(\w+)(?:=""([^""]*)""|$)");
            var attributesMatch = attributesRegex.Matches(line);
            foreach (Match attributeMatch in attributesMatch)
            {
                var attributeName = attributeMatch.Groups[1].Value;
                var attributeValue = attributeMatch.Groups[2].Value;

                if (attributeName.Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    newElement.Id = attributeValue;
                }
                else if (attributeName.Equals("class", StringComparison.OrdinalIgnoreCase))
                {
                    newElement.Classes.AddRange(attributeValue.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                }
                else if (!string.IsNullOrEmpty(attributeValue))
                {
                    newElement.Attributes.Add(attributeValue);
                }
            }

            // Handle InnerHtml
            var innerContentMatch = Regex.Match(line, @">(.*?)<");
            if (innerContentMatch.Success)
            {
                var innerContent = innerContentMatch.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(innerContent))
                {
                    newElement.InnerHtml = innerContent;
                }
            }

            // Add to the tree
            if (currentElement == null)
            {
                rootElement = newElement;
            }
            else
            {
                currentElement.Children.Add(newElement);
            }

            if (!isSelfClosing)
            {
                currentElement = newElement;
            }
        }
        else
        {
            // Handle plain text between tags
            if (currentElement != null)
            {
                currentElement.InnerHtml = line.Trim();
            }
        }
    }
    PrintTree(rootElement, 0);
    return rootElement;
}
