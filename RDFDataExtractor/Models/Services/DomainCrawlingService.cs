using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VDS.RDF;

namespace RDFDataExtractor.Models.Services
{
    public class DomainCrawlingService
    {
        private readonly TextExtractionService _textExtractionService;
        private readonly DistillationService _distillationService;
        private readonly HtmlPiecesExtractionService _piecesExtractionService;
        private readonly UtilityService _utilityService;

        public DomainCrawlingService(TextExtractionService textExtractionService, DistillationService distillationService, 
            HtmlPiecesExtractionService piecesExtractionService, UtilityService utilityService)
        {
            _textExtractionService = textExtractionService;
            _distillationService = distillationService;
            _piecesExtractionService = piecesExtractionService;
            _utilityService = utilityService;
        }
        public string CrawlDomainForStructuredData(string uri, string outputFormat)
        {
            var graph = new Graph();
            var visited = new HashSet<string>();
            var host = new Uri(uri).Host;
            GetStructuredDataFromDomain(uri, ref graph, ref visited);
            return _utilityService.GetFormattedOutput(ref graph, outputFormat);
        }
        private void GetStructuredDataFromDomain(string uri, ref Graph graph, ref HashSet<string> visited)
        {
            if (!visited.Contains(uri))
            {
                var html = _textExtractionService.ExtractTextFromURI(uri);
                var allFormats = new List<string>
                {
                    "rdfa",
                    "microdata",
                    "turtle",
                    "jsonLd"
                };
                _distillationService.DistillData(ref html, ref graph, allFormats);
                visited.Add(uri);
                var hyperlinks = _piecesExtractionService.GetHyperlinksFromDomain(ref html, uri);
                foreach (var link in hyperlinks)
                    GetStructuredDataFromDomain(uri, ref graph, ref visited);//DFS
            }
        }
    }
}
