using System;
using System.Collections.Generic;

namespace AltecSystem.Application.DTOs.Quotations
{
    public class QuotationSummaryDto
    {
        public string QuotationNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<QuotationDetailResponse> Items { get; set; }
    }
}
