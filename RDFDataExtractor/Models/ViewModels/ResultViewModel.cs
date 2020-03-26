using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDFDataExtractor.Models.ViewModels
{
    public class ResultViewModel
    {
        public string Output { get; set; }
        public ResultViewModel(string data)
        {
            Output = data;
        }
    }
}
