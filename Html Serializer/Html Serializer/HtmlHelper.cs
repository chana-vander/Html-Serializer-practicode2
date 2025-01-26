using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Html_Serializer
{
    internal class HtmlHelper
    {
        private readonly static HtmlHelper instance=new HtmlHelper();
        public static HtmlHelper Instance=> instance;
        public string[] Alltags;
        public string[] WithoutClose;

        private HtmlHelper()
        {

            string contextAll = File.ReadAllText("HtmlTags.json");
            Alltags = JsonSerializer.Deserialize<string[]>(contextAll);

            string contextClose = File.ReadAllText("HtmlVoidTags.json");
            WithoutClose = JsonSerializer.Deserialize<string[]>(contextClose);
        }
    }
}