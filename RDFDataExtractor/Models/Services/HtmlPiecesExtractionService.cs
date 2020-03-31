using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RDFDataExtractor.Models.Services
{
    public class HtmlPiecesExtractionService
    {
        public List<string> GetTurtleSections(ref string html)
            => GetRDFDataSections(ref html, "text/turtle");
        public List<string> GetJsonLdSections(ref string html)
            => GetRDFDataSections(ref html, "application/ld+json");
        public List<string> GetHyperlinksFromDomain(ref string html, HashSet<string> visited, string domain)
        {
            try
            {
                var document = new HtmlDocument();
                document.LoadHtml(html);
                return new List<string>(document.DocumentNode.Descendants("a")
                    .Where(node => node.GetAttributeValue("href", "").Contains(domain)&& 
                        !visited.Contains(node.GetAttributeValue("href", "")))
                    .Select(node => node.GetAttributeValue("href", "")));
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
            var result = new List<HtmlNode>(htmlDocument.DocumentNode.Descendants().Where(node => IsItemscope(node)));
            var i = 0;
            foreach(var node in result)
            {
                if (!HasAttribute(node, "itemid"))
                {
                    node.SetAttributeValue("itemid", $"node{i++}");
                }
            }
            return result;
        }
        public List<HtmlNode> FindPredicateObjectDescendantNodes(HtmlNode node)
        {
            var result = new List<HtmlNode>();
            foreach(var child in node.ChildNodes)
                ProcessPredicateObjectNode(child, ref result);
            return result;
        }
        private void ProcessPredicateObjectNode(HtmlNode node, ref List<HtmlNode> rdfNodes)
        {
            if (HasAttribute(node, "itemprop"))
                rdfNodes.Add(node);
            if (!IsItemscope(node))
            {
                var children = node.ChildNodes;
                foreach (var child in children)
                {
                    ProcessPredicateObjectNode(child, ref rdfNodes);
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
