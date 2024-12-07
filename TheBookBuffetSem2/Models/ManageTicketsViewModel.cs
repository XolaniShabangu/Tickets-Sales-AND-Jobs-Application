using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class ManageTicketsViewModel
    {
        public int EventId { get; set; }
        public string EventTitle { get; set; }
        public List<TicketViewModel> Tickets { get; set; }
    }

    public class TicketViewModel
    {
        public int TicketID { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int TotalTickets { get; set; }  // Total number of tickets available
        public int RemainingTickets { get; set; }  // Number of tickets remaining
    }

}