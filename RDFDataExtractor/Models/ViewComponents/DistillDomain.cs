using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDFDataExtractor.Models.ViewComponents
{
    public class DistillDomain:ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
            => await Task.FromResult(View());
    }
}
