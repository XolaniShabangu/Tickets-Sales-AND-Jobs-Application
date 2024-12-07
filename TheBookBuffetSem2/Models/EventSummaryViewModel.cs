using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBookBuffetSem2.Models
{
    public class EventSummaryViewModel
    {
        public int EventId { get; set; } // Unique identifier for the event

        public string EventName { get; set; } // Name or title of the event

        public DateTime LaunchDate { get; set; } // Date and time of the event

        public string Venue { get; set; } // Name of the venue where the event is held

        public int AttendeesCount { get; set; } // Total number of attendees who have checked in

        public int GeneralTicketsSold { get; set; } // Number of General tickets sold

        public int VIPTicketsSold { get; set; } // Number of VIP tickets sold

        public int VVIPTicketsSold { get; set; } // Number of VVIP tickets sold

        public int TotalTicketsSold => GeneralTicketsSold + VIPTicketsSold + VVIPTicketsSold; // Total tickets sold for the event

        public decimal GeneralTicketPrice { get; set; } // Price of General tickets

        public decimal VIPTicketPrice { get; set; } // Price of VIP tickets

        public decimal VVIPTicketPrice { get; set; } // Price of VVIP tickets

        public double AverageRating { get; set; } // Average rating for the event based on surveys

        public int TotalRatings { get; set; } // Total number of ratings
    }




}