using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDFDataExtractor.Models.Services
{
    public class HtmlPiecesExtractionService
    {
        public List<string> GetTurtleNodes(ref string html)
            => GetNodes(ref html, "text/turtle");
        public List<string> GetJsonLdNodes(ref string html)
            => GetNodes(ref html, "application/ld+json");
        private List<string> GetNodes(ref string html, string format)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);
            return new List<string>(document.DocumentNode.Descendants("script").Where(node => 
                node.GetAttributeValue("type", "") == format).Select(node=>node.InnerHtml));
        }
    }
}
