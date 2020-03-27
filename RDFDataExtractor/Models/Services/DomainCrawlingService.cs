using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDFDataExtractor.Models.Services
{
    public class DomainCrawlingService
    {
        private readonly TextExtractionService _textExtractionService;
        private readonly DistillationService _distillationService;
        private readonly HtmlPiecesExtractionService _piecesExtractionService;

        public DomainCrawlingService(TextExtractionService textExtractionService, DistillationService distillationService, 
            HtmlPiecesExtractionService piecesExtractionService)
        {
            _textExtractionService = textExtractionService;
            _distillationService = distillationService;
            _piecesExtractionService = piecesExtractionService;
        }
    }
}
