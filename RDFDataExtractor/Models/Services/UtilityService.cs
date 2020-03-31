using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Writing;
using HtmlAgilityPack;

namespace RDFDataExtractor.Models.Services
{
    public class UtilityService
    {
        private readonly HtmlPiecesExtractionService _piecesExtractionService;
        public UtilityService(HtmlPiecesExtractionService piecesExtractionService)
        {
            _piecesExtractionService = piecesExtractionService;
        }
        public string GetFormattedOutput(ref Graph graph, string outputFormat)
        {
            var writer = new System.IO.StringWriter();
            if (outputFormat == "jsonLd")
            {
                var jsonLdWriter = new JsonLdWriter();
                var store = new TripleStore();
                store.Add(graph);
                jsonLdWriter.Save(store, writer);
            }
            else
            {
                dynamic formattedWriter = new CompressingTurtleWriter();
                if (outputFormat == "rdf/xml")
                    formattedWriter = new RdfXmlWriter();
                else if (outputFormat == "n triples")
                    formattedWriter = new NTriplesWriter();
                formattedWriter.Save(graph, writer);
            }
            return writer.ToString();
        }
        public string GetRecognizableJsonLdObject(string startString)
        {
            var startObject = JObject.Parse(startString);
            var context = GetContext(startObject);
            startObject.Remove("@context");
            var visited = new HashSet<string>();
            HandleJsonLdObject(ref startObject, ref visited, context);
            return startObject.ToString();
        }
        public string GetMicrodataStructuredData(string html)
        {
            var allItemscopes = _piecesExtractionService.GetItemscopeNodes(html);
            var allSubjects = GetSubjectNodes(allItemscopes);
            var triples = new List<Triple>();
            for (var i = 0; i < allItemscopes.Count; i++)
            {
                var itemscope = allItemscopes[i];
                var descendants = _piecesExtractionService.FindPredicateObjectDescendantNodes(itemscope);
                if (HasAttribute(itemscope, "itemtype"))
                {
                    var nodeFactory = new NodeFactory();
                    triples.Add(new Triple(GetAppropriateIdNode(itemscope), nodeFactory.CreateUriNode(new Uri(GetTypeString())), 
                        nodeFactory.CreateUriNode(new Uri(itemscope.Attributes["itemtype"].Value))));
                }
                ExtractMicrodataTriples(triples.First().Subject, ref descendants, ref triples);
            }
            return SerializeTripleList(triples);
        }
        private INode GetAppropriateIdNode(HtmlNode itemscope)
        {
            var nodeFactory = new NodeFactory();
            return nodeFactory
                 .CreateBlankNode(itemscope.Attributes["itemid"].Value);
        }
        private void ExtractMicrodataTriples(INode currentSubject, ref List<HtmlNode> predicateObjectDescendants,
            ref List<Triple> triples)//вадење на тројки од еден точно определен itemscope
        {
            for (var i= 0; i < predicateObjectDescendants.Count; i++)
            {
                var predicateAndObject = GetPredicateAndObjectNode(predicateObjectDescendants[i]);
                try
                {
                    triples.Add(new Triple(currentSubject, predicateAndObject.Item1, predicateAndObject.Item2));
                }
                catch (Exception)
                {

                }
            }
        }
        private List<INode> GetSubjectNodes(List<HtmlNode> itemscopes)
        {
            var result = new List<INode>();
            var nodeFactory = new NodeFactory();
            foreach(var itemscope in itemscopes)
            {
                result.Add(nodeFactory.CreateUriNode(new Uri(new Uri("http://schema.org"), itemscope.Attributes["itemid"].Value)));
            }
            return result;
        }
        private string GetTypeString()
            => "http://www.w3.org/1999/02/22-rdf-syntax-ns#type";
        private IBlankNode GetNewSubjectNode()
            => new NodeFactory().CreateBlankNode();
        private Tuple<INode, INode> GetPredicateAndObjectNode(HtmlNode htmlNode)
        {
            var nodeFactory = new NodeFactory();
            try
            {
                var item1 = nodeFactory.CreateUriNode(new Uri(new Uri("http://schema.org"), htmlNode.GetAttributeValue("itemprop", null)));
                var item2 = GetObjectNode(htmlNode);
                return new Tuple<INode, INode>(item1, item2);
            }
            catch (Exception)
            {
                return null;
            }
        }
        private INode GetObjectNode(HtmlNode htmlNode)
        {
            var nodeFactory = new NodeFactory();
            if (htmlNode.Attributes["itemscope"] != null)
            {
                return nodeFactory.CreateBlankNode(htmlNode.Attributes["itemid"].Value);
            }
            else if (HasAttribute(htmlNode, "datetime"))
                return nodeFactory.CreateLiteralNode(htmlNode.Attributes["datetime"].Value);
            else if (htmlNode.OriginalName == "a")
                return nodeFactory.CreateUriNode(new Uri(htmlNode.Attributes["href"].Value));
            else if (htmlNode.OriginalName == "img")
                return nodeFactory.CreateUriNode(new Uri(htmlNode.Attributes["src"].Value));
            else if (htmlNode.OriginalName == "meta")
                return nodeFactory.CreateUriNode(new Uri(htmlNode.Attributes["content"].Value));
            else
                return nodeFactory.CreateLiteralNode(htmlNode.InnerText);
        }
        private string SerializeTripleList(List<Triple> triples)
        {
            var strings = new List<string>();
            foreach(var triple in triples)
            {
                strings.Add(GetFormattedNTripleOutput(triple));
            }
            return new StringBuilder().AppendJoin(".\n", strings).ToString()+".";
        }
        private string GetFormattedNTripleOutput(Triple triple)
        {
            var literal = triple.Object.NodeType == NodeType.Literal;
            return GetNTriplesComponentOutput(triple.Subject.ToString()) + " " + GetNTriplesComponentOutput
                (triple.Predicate.ToString()) + " " + GetNTriplesComponentOutput(triple.Object.ToString(), literal);
        }
        private string GetNTriplesComponentOutput(string component, bool literal=false)
        {
            if (component.Contains("http") && component.Contains("://"))
                return $"<{component}>";
            else if (!literal)
                return component;
            else
                return $"\"{component}\"";
        }
        private bool HasAttribute(HtmlNode node, string attributeName)
            => node.GetAttributeValue(attributeName, null) != null;
        private JToken HandleJsonLdObject(ref JObject jObject, ref HashSet<string> visited, Uri context)
        {
            if (!visited.Contains(jObject.ToString()))
            {
                visited.Add(jObject.ToString());
                HandleNormalProperties(ref jObject, context);
                HandleSpecialProperties(ref jObject, context);
                var namesForRepair = new List<string>();
                var valuesForRestore = new List<JToken>();
                var arraysForRestore = new List<string>();
                var arrayValuesForRestore = new List<List<JToken>>();
                PrepareForChange(ref jObject, ref visited, ref namesForRepair, ref valuesForRestore, ref arraysForRestore,
                    ref arrayValuesForRestore, context);
                AssembleObject(ref jObject, ref namesForRepair, ref valuesForRestore, ref arraysForRestore, ref arrayValuesForRestore);
            }
            return jObject;
        }
        private void HandleNormalProperties(ref JObject jObject, Uri context)
        {
            var properties = new List<JProperty>(jObject.Properties());
            var namesForRepair = new List<string>();
            var valuesForRestore = new List<JToken>();
            for (var i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                if (!property.Name.Contains("@"))
                {
                    namesForRepair.Add(property.Name);
                    valuesForRestore.Add(property.Value);
                }
            }
            for (var i = 0; i < namesForRepair.Count; i++)
            {
                var name = namesForRepair[i];
                jObject.Remove(name);
                jObject.Add(new Uri(context, name).ToString(), valuesForRestore[i]);
            }
        }
        private void HandleSpecialProperties(ref JObject jObject, Uri context)
        {
            var properties = new List<JProperty>(jObject.Properties());
            for (var i = 0; i < properties.Count; i++)
            {
                var name = properties[i].Name;
                var value = properties[i].Value.ToString();
                if (!name.Contains("@"))
                    continue;
                if (!Uri.IsWellFormedUriString(value, UriKind.Absolute)&&properties[i].Value.Type==JTokenType.String)
                {
                    jObject.Remove(name);
                    jObject.Add(name, new Uri(context, value).ToString());
                }
            }
        }
        private void PrepareForChange(ref JObject jObject, ref HashSet<string> visited, ref List<string> namesForRepair, 
            ref List<JToken> valuesForRestore, ref List<string> arraysForRestore, ref List<List<JToken>> arrayValuesForRestore,
            Uri context)
        {
            var properties = jObject.Properties();
            foreach (var child in properties)
            {
                if (child.Value.Type == JTokenType.Object)
                {
                    namesForRepair.Add(child.Name);
                    var obj = JObject.Parse(child.Value.ToString());
                    valuesForRestore.Add(HandleJsonLdObject(ref obj, ref visited, context));
                }
                else if (child.Value.Type == JTokenType.Array)
                {
                    var objectArray = child.Value;
                    var newTokens = new List<JToken>();
                    var children = new List<JToken>(objectArray.Children());
                    for (var i = 0; i < children.Count; i++)
                    {
                        try
                        {
                            var subchild = JObject.Parse(children[i].ToString());
                            newTokens.Add(HandleJsonLdObject(ref subchild, ref visited, context));
                        }
                        catch (Exception)
                        {
                            newTokens.Add("\"" + children[i].ToString() + "\"");
                        }
                    }
                    arraysForRestore.Add(child.Name);
                    arrayValuesForRestore.Add(newTokens);
                }
            }
        }
        private void AssembleObject(ref JObject jObject, ref List<string> namesForRepair, ref List<JToken> valuesForRestore, 
            ref List<string> arraysForRestore, ref List<List<JToken>> arrayValuesForRestore)
        {
            for (var i = 0; i < namesForRepair.Count; i++)
            {
                var name = namesForRepair[i];
                jObject.Remove(name);
                jObject.Add(name, valuesForRestore[i]);
            }
            for (var i = 0; i < arraysForRestore.Count; i++)
            {
                var name = arraysForRestore[i];
                var valueForRestore = arrayValuesForRestore[i];
                jObject.Remove(name);
                var arrayString = JsonLdArrayString(valueForRestore);
                jObject.Add(name, JArray.Parse(arrayString));
            }
        }
        private string JsonLdArrayString(List<JToken> tokens)
        {
            var builder = new StringBuilder();
            builder.Append('[');
            for (var i = 0; i < tokens.Count - 1; i++)
            {
                var token = tokens[i];
                builder.Append(token.ToString() + ",");
            }
            builder.Append(tokens[^1]);
            builder.Append(']');
            return builder.ToString();
        }
        private JValue AppropriateValue(string value)
        {
            if (Uri.IsWellFormedUriString(value, UriKind.Absolute))
                return new JValue("{" + $"\"@id\": \"{value}\"" + "}");
            return new JValue(value);
        }
        private Uri GetContext(JObject json)
            => new Uri(json.GetValue("@context").ToString());
    }
}
