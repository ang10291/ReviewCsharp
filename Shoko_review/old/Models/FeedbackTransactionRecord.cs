using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MobApp.Models
{
    public class FeedbackTransactionRecord
    {
        public int MerchantID { get; set; }
        public string TransactionUID { get; set; }
        public string Text { get; set; }
        public int StarsCount { get; set; }
        public int Recommendation { get; set; }
        public string Category { get; set; }
        public DateTime LocalDatetime { get; set; }
        public int LocalTimeOffset { get; set; }
        public int LocationID { get; set; } = 15005;
        public string TerminalID { get; set; } = "15005-001";
    }
}