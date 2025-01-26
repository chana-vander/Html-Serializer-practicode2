using Html_Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Html_Serializer
{
    internal class HtmlElement
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Attributes { get; set; } = new List<string>();
        public List<string> Classes { get; set; } = new List<string>();
        public string InnerHtml { get; set; }
        public HtmlElement Parent { get; set; }
        public List<HtmlElement> Children { get; set; } = new List<HtmlElement>();

        public HtmlElement(string id, string name, List<string> attributse, List<String> classes, string innerhtml)
        {
            Name = name;
            Id = id;
            Attributes = attributse;
            Classes = classes;
            InnerHtml = innerhtml;
        }
        public HtmlElement(string name)
        {
            Name = name;
        }
        //public HtmlElement() { }
        /*זה הבונה שלי במחלקת htmlElement:*/
        public HtmlElement()
        {

            this.Attributes = new List<string>();
            Classes = new List<string>();

            Children = new List<HtmlElement>();
        }
        
        public bool IsTag(string x)
        {
            return Array.Exists(HtmlHelper.Instance.WithoutClose, element => x.StartsWith(element)) ||
                Array.Exists(HtmlHelper.Instance.Alltags, element => x.StartsWith(element)); ;
        }
        public void AddChild(HtmlElement child)
        {
            child.Parent = this;
            Children.Add(child);
        }
        public static HtmlElement BuildTree(IEnumerable<string> htmlLines)
        {
            HtmlElement root = new HtmlElement("html");
            HtmlElement currrent = root;

            foreach (string line in htmlLines)
            {
                if (line.StartsWith('/'))
                    currrent = currrent.Parent ?? (root);
                //continue;
                string tag = ExtractTag(line);
                if (!string.IsNullOrEmpty(tag))
                {
                    HtmlElement newElement = new HtmlElement(tag);
                    currrent.AddChild(newElement);

                    string attributesPart = line.Substring(tag.Length).Trim();
                    //newElement.AddAttribute(attributesPart);

                    if (!newElement.IsSelfClosing())//
                        currrent = newElement;
                }
                else
                    currrent.InnerHtml += line.Trim();//
            }
            return root;
        }
        //תגיות שגם פותחות וגם סוגרות
        static string? ExtractTag(string line)
        {
            return HtmlHelper.Instance.Alltags.FirstOrDefault(element => line.StartsWith(element));
        }

        public bool IsSelfClosing()
        {
            // Check if the tag is self-closing
            return HtmlHelper.Instance.Alltags.Contains(Name) || Name.EndsWith("/");
        }
        public IEnumerable<HtmlElement> Descendants()
        {
            Queue<HtmlElement> queue = new Queue<HtmlElement>();
            HtmlElement current;
            queue.Enqueue(this);
            while (queue.Count > 0)
            {
                current = queue.Dequeue();
                yield return current;
                foreach (HtmlElement child in current.Children)
                {
                    queue.Enqueue(child);
                }
            }
        }
        public IEnumerable<HtmlElement> Ancestors()
        {
            HtmlElement current = this;
            while (current != null)
            {
                yield return current;
                current = current.Parent;
            }
        }
        public IEnumerable<HtmlElement> FindElements(HtmlElement element, Selector selector)
        {
            // תנאי עצירה: אם הסלקטור הנוכחי ריק, החיפוש הסתיים
            if (selector == null)
                yield break;

            // קבלת כל הצאצאים
            IEnumerable<HtmlElement> descendants = element.Descendants();

            // סינון הצאצאים לפי הקריטריונים של הסלקטור הנוכחי
            HashSet<HtmlElement> matchingElements = new HashSet<HtmlElement>(
               descendants.Where(el =>
                (string.IsNullOrEmpty(selector.TagName) || el.Name == selector.TagName) &&
                (string.IsNullOrEmpty(selector.Id) || el.Id == selector.Id) &&
                (!selector.Classes.Any() || el.Classes.Any(cls => selector.Classes.Contains(cls)))
             ));

            //return data:
            if (selector.Child == null)
            {
                foreach (HtmlElement child in matchingElements)
                    yield return child;
                yield break;
            }
            //continue find:
            foreach (HtmlElement el in matchingElements)
                foreach (HtmlElement res in FindElements(el, selector.Child))
                {
                    yield return res;
                }
        }
    }
}
