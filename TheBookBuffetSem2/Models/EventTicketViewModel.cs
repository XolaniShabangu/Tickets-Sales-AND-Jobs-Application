using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class EventTicketViewModel
    {
        public int EventID { get; set; }
        public string EventTitle { get; set; }
        public string VenueName { get; set; }
        public string VenueLocation { get; set; }
        public DateTime EventDate { get; set; }
        public string ImageUrl { get; set; }
        public List<TicketDetailViewModel> Tickets { get; set; }
        public decimal MinPrice { get; set; }
    }

    public class TicketDetailViewModel
    {
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int QuantityLeft { get; set; }
    }

    

    public class EventDetailsViewModel
    {
        public Event Event { get; set; }
        public Venue Venue { get; set; }
        public List<TicketViewModel> Tickets { get; set; }
    }

   
}