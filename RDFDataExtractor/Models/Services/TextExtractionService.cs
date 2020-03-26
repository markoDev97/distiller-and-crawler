using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace RDFDataExtractor.Models.Services
{
    public class TextExtractionService
    {
        public string ExtractTextFromURI(string uri)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            var response = (HttpWebResponse)request.GetResponse();
            var streamReader = 
        }
    }
}
