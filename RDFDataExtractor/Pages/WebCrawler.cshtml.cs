using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RDFDataExtractor.Models.Services;

namespace RDFDataExtractor.Pages
{
    public class WebCrawlerModel : PageModel
    {
        private readonly DomainCrawlingService _crawlingService;

        public WebCrawlerModel(DomainCrawlingService crawlingService)
        {
            _crawlingService = crawlingService;
        }
        public bool ShowOutput { get; private set; }
        public string OutputData { get; private set; }
        public void OnGet()
        {

        }
        public void OnPost(string uri, string outputFormat)
        {
            OutputData = _crawlingService.CrawlDomainForStructuredData(uri, outputFormat);
            ShowOutput = true;
        }
    }
}