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
        public List<HtmlNode> GetItemscopeNodes(string html)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            return new List<HtmlNode>(htmlDocument.DocumentNode.Descendants().Where(node => IsItemscope(node)));
        }
        public List<HtmlNode> FindPredicateObjectDescendantNodes(HtmlNode node)
        {
            var result = new List<HtmlNode>();
            ProcessPredicateObjectNode(node, ref result);
            return result;
        }
        private void ProcessPredicateObjectNode(HtmlNode node, ref List<HtmlNode> rdfNodes)
        {
            rdfNodes.Add(node);
            if (!IsItemscope(node))
            {
                var descendants = node.Descendants();
                foreach (var descendant in descendants)
                {
                    if (HasAttribute(descendant, "itemprop")||HasAttribute(descendant, "itemscope"))
                        ProcessPredicateObjectNode(descendant, ref rdfNodes);
                }
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
        private bool IsItemscope(HtmlNode node)
            => node.GetAttributeValue("itemscope", null) != null;
        private bool HasAttribute(HtmlNode node, string attributeName)
            => node.GetAttributeValue(attributeName, null) != null;
    }
}
