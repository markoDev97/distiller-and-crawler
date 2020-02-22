using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace RDFDataExtractor.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public bool ShowOutput { get; private set; }
        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
        public void OnPostForDistillation(string file_for_distillation, string page_uri, string raw_input, string rdfa, 
            string microdata, string turtle, string json_ld, string format)
        {
            ShowOutput = true;
        }
    }
}
