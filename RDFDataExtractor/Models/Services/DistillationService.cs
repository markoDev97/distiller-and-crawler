using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace RDFDataExtractor.Models.Services
{
    public class DistillationService
    {
        private readonly HtmlPiecesExtractionService _htmlPiecesExtractionService;

        public DistillationService(HtmlPiecesExtractionService htmlPiecesExtractionService)
        {
            _htmlPiecesExtractionService = htmlPiecesExtractionService;
        }
        public string DistillDataInFormat(ref string html, List<string> inputFormats, string outputFormat)
        {//приватни методи за соодветно дестилирање
            var graph = new Graph();
            foreach (var format in inputFormats)
                FillGraph(ref graph, ref html, format);
            return GetFormattedOutput(ref graph, outputFormat);
        }
        private string GetFormattedOutput(ref Graph graph, string outputFormat)
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
        private void FillGraph(ref Graph graph, ref string html, string inputFormat)
        {
            if (inputFormat == "rdfa")
                ExtractRDFaStructuredData(ref html, ref graph);
            else if (inputFormat == "microdata")
                ExtractMicrodataStructuredData(ref html, ref graph);
            else if (inputFormat == "jsonLd")
                ExtractJsonLdStructuredData(ref html, ref graph);
            else if (inputFormat == "turtle")
                ExtractTurtleStructuredData(ref html, ref graph);
        }
        //кај extract методите додај механизми за вадење на делови од интерес по потреба
        private void ExtractRDFaStructuredData(ref string html, ref Graph graph)
        {
            graph.LoadFromString(html, new RdfAParser());
        }
        private void ExtractMicrodataStructuredData(ref string html, ref Graph graph)
        {
            
        }
        private void ExtractJsonLdStructuredData(ref string html, ref Graph graph)
        {
            var parser = new JsonLdParser();
            var store = new TripleStore();
            store.Add(graph);
            var jsonPieces = _htmlPiecesExtractionService.GetJsonLdNodes(ref html);
            foreach(var piece in jsonPieces)
            {
                try
                {
                    parser.Load(store, new StringReader(piece));//овде за жал допроцесирање на парчето json
                }
                catch (Exception)
                {

                }
            }
        }
        private void ExtractTurtleStructuredData(ref string html, ref Graph graph)
        {
            var turtlePieces = _htmlPiecesExtractionService.GetTurtleNodes(ref html);
            foreach(var piece in turtlePieces)
                graph.LoadFromString(piece, new TurtleParser());
        }
    }
}
