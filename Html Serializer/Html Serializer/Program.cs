using Html_Serializer;
using System.Text.RegularExpressions;
//מהאתר מכבי html שליפת 
//var html = await Load("https://www.maccabi4u.co.il");
//var html = await Load("https://chat.malkabruk.co.il/");
//var html = await Load("https://www.matara.pro/nedarimplus/");
static async Task<string> Load(string url)
{
    HttpClient client = new HttpClient();
    var response = await client.GetAsync(url);
    var html = await response.Content.ReadAsStringAsync();
    return html;
}
static HtmlElement BuildTree(string html)
{
    //cleaning html
    var cleanHtml = Regex.Replace(html, @"<!--[\s\S]*?-->", "");
    cleanHtml = Regex.Replace(cleanHtml, @"\s*\n\s*|\s{2,}", " "); // הסרת רווחים מיותרים
    //split the html to lines as tags list
    var htmlLines = new Regex("<(.*?)>").Split(cleanHtml).Where(s => s.Length > 0);
    HtmlElement root = null;
    HtmlElement current = null;

    foreach (var line in htmlLines)
    {
        //מילה ראשונה מהשורה
        var firstWord = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        if (string.IsNullOrEmpty(firstWord))
            continue;

        if (firstWord.StartsWith("/") && HtmlHelper.Instance.Alltags.Contains(firstWord.Substring(1)))
        {
            //תו סוגר -לשים באלמנט נוכחי את האבא
            if (current != null)
                current = current.Parent;
            continue;
        }
        if (IsTagHtml(firstWord))
        {
            //crete a new element
            var tagName = firstWord.TrimEnd('/');//delete /
            bool isSelfClosing = firstWord.EndsWith("/") || HtmlHelper.Instance.WithoutClose.Contains(tagName);//אם זו תגית סגירה

            HtmlElement newElement = new HtmlElement
            {
                Name = tagName,
                Parent = current
            };

            AddAttribute(line, newElement);

            //innerHtml
            var innerMatch = Regex.Match(line, @">(.*?)<");
            if (innerMatch.Success)
            {
                var innerCon = innerMatch.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(innerCon))
                    newElement.InnerHtml = innerCon;
            }
            //add to tree
            if (current == null)
                root = newElement;

            else
                current.Children.Add(newElement);

            if (!isSelfClosing)
                current = newElement;
        }
        else
        {
            if (current != null)
                current.InnerHtml = line.Trim();
        }
    }
    PrintTree(root, 0);
    return root;
}
static bool IsTagHtml(string word1)
{
    return (HtmlHelper.Instance.Alltags.Contains(word1, StringComparer.OrdinalIgnoreCase)
            || HtmlHelper.Instance.WithoutClose.Contains(word1));
}
static void AddAttribute(string line, HtmlElement newElement)
{
    var attributes = Regex.Matches(line, @"(\w+)(?:=(""[^""]*""|'[^']*')|)");
    foreach (Match attr in attributes)
    {
        string attrName = attr.Groups[1].Value;//name
        string attrValue = attr.Groups[2].Success ? attr.Groups[2].Value.Trim('"', '\'') : string.Empty;


        if (attrName.Equals("class", StringComparison.OrdinalIgnoreCase))
            newElement.Classes.AddRange(attrValue.Split(' ', StringSplitOptions.RemoveEmptyEntries));//אם יש כמה קלאסים?

        else if (attrName.Equals("id", StringComparison.OrdinalIgnoreCase))
            newElement.Id = attrValue;

        else if (!string.IsNullOrEmpty(attrName))
        {
            if (string.IsNullOrEmpty(attrValue))
                newElement.Attributes.Add(attrName);
            else
                newElement.Attributes.Add($"{attrName}=\"{attrValue}\"");
        }
    }
}
//הצגת העץ
// הצגת העץ
static void PrintTree(HtmlElement element, int level)
{
    var indent = new string(' ', level * 2);
    Console.WriteLine($"{indent}{element.Name} (ID: {element.Id})");

    if (!string.IsNullOrEmpty(element.InnerHtml))
        Console.WriteLine($"{indent}  InnerHtml: {element.InnerHtml}");

    if (element.Attributes.Count > 0)
        Console.WriteLine($"{indent}  Attributes: {string.Join(", ", element.Attributes)}");

    if (element.Classes.Count > 0)
        Console.WriteLine($"{indent}  Classes: {string.Join(", ", element.Classes)}");

    foreach (var child in element.Children)
        PrintTree(child, level + 1);

    if (element.Children.Count > 0)
        Console.WriteLine($"{indent}</{element.Name}>");
}
/*static void PrintTree(HtmlElement element, int level)
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
}*/

//main:
var html = await Load("https://www.matara.pro/nedarimplus/");
BuildTree(html);
Selector selector = Selector.Convert("div img");
Console.WriteLine(selector.ToString());

//var res1 = BuildTree(html);
//var res2 = res1.FindElements(res1, selector);