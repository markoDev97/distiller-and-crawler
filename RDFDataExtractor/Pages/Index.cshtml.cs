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
        private readonly DistillationService _distillationService;
        private readonly TextExtractionService _extractionService;
        public bool ShowOutput { get; private set; }
        public string WhichApproach { get;  private set; }
        public string OutputData { get; private set; }
        public IndexModel(DistillationService distillationService, TextExtractionService extractionService)
        {
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
            string microdata, string turtle, string jsonLd, string outputFormat)
        {
            if (fileForDistillation.ContentType == "text/html")
            {
                var html = _extractionService.ExtractTextFromFile(fileForDistillation);
                WorkWithMarkup(ref html, rdfa, microdata, turtle, jsonLd, outputFormat);
            }
            else
            {
                ViewData["error"] = "Only .html files can be uploaded.";
            }
        }
        public void OnPostURI(string pageUri, string rdfa, string microdata, string turtle, string jsonLd, string outputFormat)
        {
            var html = _extractionService.ExtractTextFromURI(pageUri);
            WorkWithMarkup(ref html, rdfa, microdata, turtle, jsonLd, outputFormat);
        }
        public void OnPostRawText(string rawInput, string rdfa, string microdata, string turtle, string jsonLd, string outputFormat)
        {
            WorkWithMarkup(ref rawInput, rdfa, microdata, turtle, jsonLd, outputFormat);
        }
        private void WorkWithMarkup(ref string markup, string rdfa, string microdata, string turtle, 
            string jsonLd, string outputFormat)
        {
            var inputFormats = InputFormats(rdfa, microdata, turtle, jsonLd);
            OutputData = _distillationService.DistillDataInFormat(ref markup, inputFormats, outputFormat);
            ShowOutput = true;
        }
        private List<string> InputFormats(string rdfa, string microdata, string turtle, string jsonLd)
        {
            var res = new List<string>();
            FillFormatList(ref res, rdfa, microdata, turtle, jsonLd);
            return res;
        }
        private void FillFormatList(ref List<string> list, string rdfa, string microdata, string turtle, string jsonLd)
        {
            AddIfOn(ref list, "rdfa", rdfa);
            AddIfOn(ref list, "microdata", microdata);
            AddIfOn(ref list, "turtle", turtle);
            AddIfOn(ref list, "jsonLd", jsonLd);
        }
        private void AddIfOn(ref List<string> set, string format, string value)
        {
            if (value == "on")
                set.Add(format);
        }
    }
}
