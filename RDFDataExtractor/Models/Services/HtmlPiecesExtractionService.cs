using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDFDataExtractor.Models.Services
{
    public class HtmlPiecesExtractionService
    {
        public List<string> GetTurtleSections(ref string html)
            => GetRDFDataSections(ref html, "text/turtle");
        public List<string> GetJsonLdSections(ref string html)
            => GetRDFDataSections(ref html, "application/ld+json");
        public List<string> GetHyperlinksFromDomain(ref string html, string domain)
        {
            try
            {
                var document = new HtmlDocument();
                document.LoadHtml(html);
                return new List<string>(document.DocumentNode.Descendants("a").Where(node => node.GetAttributeValue("href", "")
                    .Contains(domain)).Select(node => node.GetAttributeValue("href", "")));
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }
        private List<string> GetRDFDataSections(ref string html, string format)
        {
            try
            {
                var document = new HtmlDocument();
                document.LoadHtml(html);
                return new List<string>(document.DocumentNode.Descendants("script").Where(node =>
                    node.GetAttributeValue("type", "") == format).Select(node => node.InnerHtml));
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }
    }
}
