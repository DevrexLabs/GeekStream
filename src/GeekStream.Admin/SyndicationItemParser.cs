using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using GeekStream.Core;
using HtmlAgilityPack;

namespace GeekStream.Admin
{
    public class SyndicationItemParser
    {

        static Regex _keywordSplitter = new Regex(@"\W+");

        private SyndicationItem _item;
        private string _summary;
        private string[] _keywords;

        public SyndicationItemParser(SyndicationItem syndicationItem)
        {
            _item = syndicationItem;
            Parse();
        }


        public string Summary
        {
            get { return _summary; }
        }

        public string[] Keywords
        {
            get { return _keywords; }
        }
        
        private void Parse()
        {
            var builder = new StringBuilder();
            if (_item.Summary != null)
            {
                string text = _item.Summary.Text;
                text = HttpUtility.HtmlDecode(text);
                StripHtml(ref text);
                builder.Append(text).Append(" ");
            }

            if (_item.Content != null)
            {
                string mainContent = GetContent(_item.Content);
                builder.Append(mainContent);
            }

            builder.Append(" ").Append(_item.Title.Text);
            string contentToIndex = builder.ToString().Trim();

            _summary = contentToIndex;
            if (_summary.Length > 200) _summary = _summary.Substring(0, 197) + "...";

            var words = _keywordSplitter.Split(contentToIndex);
            _keywords = new HashSet<string>(words, StringComparer.InvariantCultureIgnoreCase).ToArray();
        }


        private static void StripHtml(ref string text)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(text);
            
            //This included html comments...
            //text = doc.DocumentNode.InnerText;

            StringBuilder builder = new StringBuilder();
            foreach (HtmlNode textNode in doc.DocumentNode.SelectNodes("//text()"))
            {
                builder.Append(textNode.InnerText).Append(" ");
            }
            text = builder.ToString();

        }

        private static string GetContent(SyndicationContent syndicationContent)
        {
            
            var sw = new StringWriter();
            sw.Write(" ");
            XmlWriter writer = new XmlTextWriter(sw);
            syndicationContent.WriteTo(writer, "whatever", "");
            string text = sw.ToString();
            text = HttpUtility.HtmlDecode(text);
            StripHtml(ref text);
            return text;
        }
    }
}
