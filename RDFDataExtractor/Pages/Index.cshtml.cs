using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RDFDataExtractor.Models.Services;

namespace RDFDataExtractor.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly DistillationService _distillationService;
        private readonly TextExtractionService _extractionService;

        public bool ShowOutput { get; private set; }
        public string WhichApproach { get;  private set; }
        public IndexModel(ILogger<IndexModel> logger, DistillationService distillationService, TextExtractionService extractionService)
        {
            _logger = logger;
            _distillationService = distillationService;
            _extractionService = extractionService;
        }

        public void OnGet()
        {

        }
        public void OnPostWhichApproach(string whichApproach)
        {
            WhichApproach = whichApproach;
        }
        public void OnPostFile(IFormFile fileForDistillation, string pageUri, string rawInput, string rdfa,
            string microdata, string turtle, string jsonLd, string format)
        {
            if (fileForDistillation.ContentType == "text/html")
            {
                var html = _extractionService.ExtractTextFromFile(fileForDistillation);
            }
            else
            {
                ViewData["error"] = "Only .html files can be uploaded.";
            }
        }
        public void OnPostURI(string pageUri, string rdfa, string microdata, string turtle, string jsonLd, string format)
        {
            var html = _extractionService.ExtractTextFromURI(pageUri);
            ShowOutput = true;
        }
        public void OnPostRawText(string rawInput, string rdfa, string microdata, string turtle, string jsonLd, string format)
        {
            ShowOutput = true;
        }
        private ISet<string> WantedFormats(string rdfa, string microdata, string turtle, string jsonLd)
        {
            var res = new HashSet<string>();
            return res;
        }
        private void AddIfOn(ref ISet<string> set, string format)
        {
            if (format == "on")
                set.Add(format);
        }
    }
}
