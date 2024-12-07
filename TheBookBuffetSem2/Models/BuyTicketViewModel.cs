using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class BuyTicketViewModel
    {
        public int EventID { get; set; }
        public string EventTitle { get; set; }
        public List<TicketViewModel> Tickets { get; set; }
    }
}