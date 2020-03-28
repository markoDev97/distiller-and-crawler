using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace RDFDataExtractor.Models.Services
{
    public class TextExtractionService
    {
        public string ExtractTextFromURI(string uri)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(uri);
                var response = (HttpWebResponse)request.GetResponse();
                using var streamReader = new StreamReader(response.GetResponseStream());
                return streamReader.ReadToEnd();
            }
            catch (Exception)
            {
                return null;
            }
        }
        public string ExtractTextFromFile(IFormFile file)
        {
            var stream = new MemoryStream();
            file.CopyTo(stream);
            return new string(Encoding.ASCII.GetChars(stream.ToArray()));
        }
    }
}
