using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Html_Serializer
{
    internal class Selector
    {
        public string TagName { get; set; }
        public string Id { get; set; }
        public List<string> Classes { get; set; } = new List<string>();
        public Selector Parent { get; set; }
        public Selector Child { get; set; }
        public static Selector Convert(string str)
        {
            string start, mid;
            bool flag = false;
            string[] collectiontStr = str.Split(' ');
            Selector root = new Selector();
            Selector current = root;
            foreach (string s in collectiontStr)
            {
                Selector newSelector = new Selector();
                if (s.StartsWith("#"))
                {
                    start = s.Substring(0, 1);
                    mid = s.Substring(1, s.Length - 1);
                    newSelector.Id = mid ;
                    //flag = false;
                }
                else if (s.StartsWith("."))
                {
                    start = s.Substring(0, 1);
                    mid = s.Substring(1, s.Length - 1);
                    newSelector.Classes.Add(mid);
                    //flag = true;
                }
                /*else if(flag)
                {
                    current.Classes.Add(s);
                }*/
                else//אם הוא שם של תגית 
                {
                    if (Selector.IsTag(s))
                    {
                        newSelector.TagName = s;
                    }
                }
                current.Child = newSelector;
                newSelector.Parent = current;
                current = newSelector;
            }
            return root;
        }
        public static bool IsTag(string x)
        {
            return Array.Exists(HtmlHelper.Instance.WithoutClose, element => x.StartsWith(element)) ||
                Array.Exists(HtmlHelper.Instance.Alltags, element => x.StartsWith(element)); ;
        }



    }
}
