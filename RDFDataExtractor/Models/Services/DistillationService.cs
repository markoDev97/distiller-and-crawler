using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using VDS.RDF.Parsing;

namespace RDFDataExtractor.Models.Services
{
    public class DistillationService
    {
        public string DistillDataInFormat(string html, string format)
        {//приватни методи за соодветно дестилирање
            
            return format;
        }
        public string DistillDataFromURI(string URI)//crawler
        {
            return "";
        }
        private string ExtractRDFaStructuredData(string html)
        {
            var parser = new RdfAParser();
        }
    }
}
