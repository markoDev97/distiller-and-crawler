using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Writing;

namespace RDFDataExtractor.Models.Services
{
    public class UtilityService
    {
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
        public string GetValidJsonLdObject(string startString)
        {
            var startObject = JObject.Parse(startString);
            var context = GetContext(startObject);
            startObject.Remove("@context");
            var visited = new HashSet<string>();
            HandleJsonLdObject(ref startObject, ref visited, context);
            return startObject.ToString();
        }
        private JToken HandleJsonLdObject(ref JObject jObject, ref HashSet<string> visited, Uri context)
        {//refactor
            if (!visited.Contains(jObject.ToString()))
            {
                visited.Add(jObject.ToString());
                HandleNormalProperties(ref jObject, context);
                HandleSpecialProperties(ref jObject, context);
                var namesForRepair = new List<string>();
                var valuesForRestore = new List<JToken>();
                var arraysForRestore = new List<string>();
                var arrayValuesForRestore = new List<List<JToken>>();
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
                if (!Uri.IsWellFormedUriString(value, UriKind.Absolute))
                {
                    jObject.Remove(name);
                    jObject.Add(name, new Uri(context, value).ToString());
                }
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
