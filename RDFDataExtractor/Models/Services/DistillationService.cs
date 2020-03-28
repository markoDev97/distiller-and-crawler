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
        private readonly UtilityService _utilityService;

        public DistillationService(HtmlPiecesExtractionService htmlPiecesExtractionService, UtilityService utilityService)
        {
            _htmlPiecesExtractionService = htmlPiecesExtractionService;
            _utilityService = utilityService;
        }
        public void DistillData(ref string html, ref Graph graph, List<string> inputFormats)
        {
            foreach (var format in inputFormats)
                FillGraph(ref graph, ref html, format);
        }
        public Graph DistillData(ref string html, List<string> inputFormats)
        {
            var graph = new Graph();
            foreach (var format in inputFormats)
                FillGraph(ref graph, ref html, format);
            return graph;
        }
        public string DistillDataInFormat(ref string html, List<string> inputFormats, string outputFormat)
        {//приватни методи за соодветно дестилирање
            var graph = DistillData(ref html, inputFormats);
            return _utilityService.GetFormattedOutput(ref graph, outputFormat);
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
            try
            {
                graph.LoadFromString(html, new RdfAParser());
            }
            catch (Exception)
            {

            }
        }
        private void ExtractMicrodataStructuredData(ref string html, ref Graph graph)
        {
            
        }
        private void ExtractJsonLdStructuredData(ref string html, ref Graph graph)
        {
            var parser = new JsonLdParser();
            var store = new TripleStore();
            store.Add(graph);
            try
            {
                var jsonPieces = _htmlPiecesExtractionService.GetJsonLdSections(ref html);
                foreach (var piece in jsonPieces)
                {
                    try
                    {
                        parser.Load(store, new StringReader(_utilityService.GetRecognizableJsonLdObject(piece)));//овде за жал допроцесирање на парчето json
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            catch (Exception)
            {

            }
        }
        private void ExtractTurtleStructuredData(ref string html, ref Graph graph)
        {
            var turtlePieces = _htmlPiecesExtractionService.GetTurtleSections(ref html);
            try
            {
                foreach (var piece in turtlePieces)
                    graph.LoadFromString(piece, new TurtleParser());
            }
            catch (Exception)
            {

            }
        }
    }
}
