using Microsoft.AspNetCore.Mvc;
using RDFDataExtractor.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDFDataExtractor.Models
{
    public class DataOutput:ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string data)
            => await Task.FromResult(View(new ResultViewModel(data)));
    }
}
